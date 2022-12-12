using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Runtime.ScriptableObjects;
using WUI.Utilities;

namespace WUI.Editor.Graph
{
    using Feature;
    using Data.Save;
    using Data.Error;
    using Elements;
    using Enumerations;
    using Utilities;
    using Manipulator;
    using Window;
    
    public class WUI_GraphView : GraphView
    {
        private readonly WUI_EditorWindow _editorWindow;
        private WUI_SearchWindow _searchWindow;

        private readonly WUI_SerializableDictionary<string, WUI_NodeErrorData> _ungroupedNodes;
        private readonly WUI_SerializableDictionary<string, WUI_GroupErrorData> _groups;
        private readonly WUI_SerializableDictionary<WUI_Group, WUI_SerializableDictionary<string, WUI_NodeErrorData>> _groupedNodes;

        private int _namesErrorAmount;

        private readonly WUI_IManipulator _manipulator;
        
        private readonly WUI_Graph_Feature _miniMap;
        private readonly WUI_Graph_Feature _gridBackground;

        public int NamesErrorAmount
        {
            get => _namesErrorAmount;
            set
            {
                _namesErrorAmount = value;

                switch (_namesErrorAmount)
                {
                    case 0:
                        _editorWindow.GetToolbar().SaveButtonEnableState(true);
                        break;
                    case 1:
                        _editorWindow.GetToolbar().SaveButtonEnableState(false);
                        break;
                }
            }
        }
        
        public WUI_GraphView(WUI_EditorWindow editorWindow)
        {
            RegisterCallback<MouseUpEvent>(_ =>
            {
                if (selection.Count != 1)
                {
                    Selection.activeObject = null;
                    return;
                }

                if (selection[0] is not WUI_Node node) return;

                var uiSO = ScriptableObject.CreateInstance<WUI_UI_SO>();
                
                uiSO.NextUI = node.NextUI;
                uiSO.PreviousUI = node.PreviousUI;
                uiSO.UIName = node.UIName;
                uiSO.UIInformation = node.UIInformation;
                uiSO.NodeType = node.NodeType;

                Selection.activeObject = uiSO;

            }, TrickleDown.TrickleDown);
            
            _editorWindow = editorWindow;

            #region Nodes

            _ungroupedNodes = new WUI_SerializableDictionary<string, WUI_NodeErrorData>();
            
            _groupedNodes = new WUI_SerializableDictionary<WUI_Group, WUI_SerializableDictionary<string, WUI_NodeErrorData>>();

            #endregion

            #region Groups

            _groups = new WUI_SerializableDictionary<string, WUI_GroupErrorData>();

            #endregion

            #region Manipulators

            _manipulator = new WUI_GraphView_Manipulator();

            #endregion

            #region Features

            _miniMap = new WUI_Graph_MiniMap();
            _gridBackground = new WUI_Graph_GridBackground();

            #endregion

            AddManipulators();
            
            AddGridBackground();

            AddSearchWindow();

            AddMiniMap();

            OnElementsDeleted();

            OnGroupElementsAdded();

            OnGroupElementsRemoved();

            OnGroupRenamed();

            OnGraphViewChanged();
            
            AddStyles();
        }

        public void LoadData(string filePath)
        {
            _editorWindow.GetToolbar().LoadToInputPath(filePath);
        }
        
        #region Override Methods

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(p =>
            {
                if (startPort == p) return;

                if (startPort.node == p.node) return;

                if (startPort.direction == p.direction) return;
                
                compatiblePorts.Add(p);
            });

            return compatiblePorts;
        }

        #endregion
        
        #region Manipulators

        private void AddManipulators()
        {
            #region Menu Manipulators

            _manipulator.AddManipulator(this, CreateNodeContextualMenu("Add First UI", WUI_NodeType.HomeUI));
            _manipulator.AddManipulator(this, CreateNodeContextualMenu("Add Middle UI", WUI_NodeType.BasicUI));
            _manipulator.AddManipulator(this, CreateNodeContextualMenu("Add Last UI", WUI_NodeType.LastUI));
            
            _manipulator.AddManipulator(this, CreateGroupContextualMenu());

            #endregion
            
            _manipulator.SetManipulator(this);
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, WUI_NodeType nodeType)
        {
            var contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(nodeType.ToString(), nodeType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));

            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            var contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("UI Group", GetLocalMousePosition(actionEvent.eventInfo.mousePosition))));

            return contextualMenuManipulator;
        }

        #endregion

        #region Elements Creation

        public WUI_Group CreateGroup(string title, Vector2 mousePosition)
        {
            var group = new WUI_Group(title, mousePosition);
            
            AddGroup(group);
            
            AddElement(group);

            #region Automatical Nodes Addiction

            foreach (var selectedElement in selection)
            {
                if (selectedElement is not WUI_Node node) continue;
                
                group.AddElement(node);
            }

            #endregion

            return group;
        }

        public Node CreateNode(string nodeName, WUI_NodeType nodeType, Vector2 position, bool shouldDraw = true)
        {
            var type = Type.GetType($"WUI.Editor.Elements.WUI_{nodeType}_Node");

            if (type == null) return default;

            if (Activator.CreateInstance(type) is not WUI_Node node) return default;

            node.Initialize(nodeName, this, nodeType, position);
            
            if(shouldDraw) node.Draw();

            AddUngroupedNode(node);
            
            AddElement(node);

            return node;
        }

        #endregion

        #region Callbacks

        private void OnElementsDeleted()
        {
            deleteSelection = (_, _) =>
            {
                var groupType = typeof(WUI_Group);

                var edgeType = typeof(Edge);
                
                var groupsToDelete = new List<WUI_Group>();

                var edgesToDelete = new List<Edge>();
                
                var nodesToDelete = new List<WUI_Node>();

                foreach (var element in selection.Cast<GraphElement>())
                {
                    if (element is WUI_Node node)
                    {
                        nodesToDelete.Add(node);
                        
                        continue;
                    }

                    if (element.GetType() == edgeType)
                    {
                        var edge = element as Edge;
                        
                        edgesToDelete.Add(edge);

                        continue;
                    }
                    
                    if (element.GetType() != groupType) continue;

                    var group = element as WUI_Group;

                    groupsToDelete.Add(group);
                }

                foreach (var group in groupsToDelete)
                {
                    var groupNodes = new List<WUI_Node>();

                    foreach (var groupElement in group.containedElements)
                    {
                        if (groupElement is not WUI_Node groupNode) continue;

                        groupNodes.Add(groupNode);
                    }
                    
                    group.RemoveElements(groupNodes);
                    
                    RemoveGroup(group);
                    
                    RemoveElement(group);
                }

                DeleteElements(edgesToDelete);
                
                foreach (var node in nodesToDelete)
                {
                    if (node.Group != null)
                    {
                        node.Group.RemoveElement(node);
                    }
                    
                    RemoveUngroupedNode(node);
                    
                    node.DisconnectAllPorts();

                    RemoveElement(node);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (var element in elements)
                {
                    if(element is not WUI_Node node) continue;
                    
                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, group as WUI_Group);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (var element in elements)
                {
                    if (element is not WUI_Node node) continue;

                    RemoveGroupedNode(node, group as WUI_Group);
                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                if (group is not WUI_Group wui_group) return;

                wui_group.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();
                
                if (string.IsNullOrEmpty(wui_group.title))
                {
                    if (!string.IsNullOrEmpty(wui_group.OldTitle))
                    {
                        ++NamesErrorAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(wui_group.OldTitle))
                    {
                        --NamesErrorAmount;
                    }
                }
                
                RemoveGroup(wui_group);

                wui_group.OldTitle = wui_group.title;
                
                AddGroup(wui_group);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (var edge in changes.edgesToCreate)
                    {
                        var previousNode = edge.output.node as WUI_Node;
                        var nextNode = edge.input.node as WUI_Node;

                        if (edge.input.userData is WUI_UISaveData nextNodeData)
                        {
                            nextNodeData.NodeID = previousNode?.ID;
                            nextNodeData.Text = previousNode?.UIName;
                        }

                        if ( edge.output.userData is WUI_UISaveData previousNodeData)
                        {
                            previousNodeData.NodeID = nextNode?.ID;
                            previousNodeData.Text = nextNode?.UIName;
                        }
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    foreach (var element in changes.elementsToRemove.Where(element => element.GetType() == typeof(Edge)))
                    {
                        if(element is not Edge edge) continue;

                        if (edge.output.userData is WUI_UISaveData outputUIData)
                        {
                            outputUIData.Text = "";
                            outputUIData.NodeID = "";
                        }
                        
                        if (edge.input.userData is WUI_UISaveData inputUIData)
                        {
                            inputUIData.Text = "";
                            inputUIData.NodeID = "";   
                        }
                    }
                }

                return changes;
            };
        }

        #endregion
        
        #region Repeated Elements

        public void AddUngroupedNode(WUI_Node node)
        {
            var nodeName = node.UIName.ToLower();

            if (!_ungroupedNodes.ContainsKey(nodeName))
            {
                var nodeErrorData = new WUI_NodeErrorData();
                nodeErrorData.Nodes.Add(node);
                
                _ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            var ungroupedNodesList = _ungroupedNodes[nodeName].Nodes;
            
            ungroupedNodesList.Add(node);

            var errorColor = _ungroupedNodes[nodeName].ErrorData.Color;
            
            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count != 2) return;
            
            ++NamesErrorAmount;
                
            _ungroupedNodes[nodeName].Nodes[0].SetErrorStyle(errorColor);
        }

        public void RemoveUngroupedNode(WUI_Node node)
        {
            var nodeName = node.UIName.ToLower();

            var ungroupedList = _ungroupedNodes[nodeName].Nodes;

            ungroupedList.Remove(node);
            
            node.ResetStyle();

            switch (ungroupedList.Count)
            {
                case 1:
                    --NamesErrorAmount;
                
                    _ungroupedNodes[nodeName].Nodes[0].ResetStyle();

                    return;
                case 0:
                    _ungroupedNodes.Remove(nodeName);
                    break;
            }
        }
        
        private void AddGroup(WUI_Group group)
        {
            var groupName = group.title.ToLower();

            if (!_groups.ContainsKey(groupName))
            {
                var groupErrorData = new WUI_GroupErrorData();
                
                groupErrorData.Groups.Add(group);
                
                _groups.Add(groupName, groupErrorData);

                return;
            }

            var groupList = _groups[groupName].Groups;
            
            groupList.Add(group);

            var errorColor = _groups[groupName].ErrorData.Color;
            
            group.SetErrorStyle(errorColor);

            if (groupList.Count != 2) return;
            
            ++NamesErrorAmount;
                
            groupList[0].SetErrorStyle(errorColor);
        }

        public void RemoveGroup(WUI_Group group)
        {
            var oldGroupName = group.OldTitle.ToLower();

            var groupList = _groups[oldGroupName].Groups;

            groupList.Remove(group);
            
            group.ResetStyle();

            switch (groupList.Count)
            {
                case 1:
                    --NamesErrorAmount;
                
                    groupList[0].ResetStyle();
                
                    return;
                case 0:
                    _groups.Remove(oldGroupName);
                    break;
            }
        }

        public void AddGroupedNode(WUI_Node node, WUI_Group group)
        {
            var nodeName = node.UIName.ToLower();

            node.Group = group;
            
            if (!_groupedNodes.ContainsKey(group))
            {
                _groupedNodes.Add(group, new WUI_SerializableDictionary<string, WUI_NodeErrorData>());
            }

            if (!_groupedNodes[group].ContainsKey(nodeName))
            {
                var nodeErrorData = new WUI_NodeErrorData();
                
                nodeErrorData.Nodes.Add(node);
                
                _groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            var groupedNodesList = _groupedNodes[group][nodeName].Nodes;
            
            groupedNodesList.Add(node);

            var errorColor = _groupedNodes[group][nodeName].ErrorData.Color;
            
            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count != 2) return;
            
            ++NamesErrorAmount;
                
            groupedNodesList[0].SetErrorStyle(errorColor);
        }
        
        public void RemoveGroupedNode(WUI_Node node, WUI_Group group)
        {
            var nodeName = node.UIName.ToLower();

            node.Group = null;
            
            var groupedNodesList = _groupedNodes[group][nodeName].Nodes;
            
            groupedNodesList.Remove(node);
            
            node.ResetStyle();

            switch (groupedNodesList.Count)
            {
                case 1:
                    --NamesErrorAmount;
                
                    groupedNodesList[0].ResetStyle();
                
                    return;
                case 0:
                {
                    _groupedNodes[group].Remove(nodeName);

                    if (_groupedNodes[group].Count == 0)
                    {
                        _groupedNodes.Remove(group);
                    }

                    break;
                }
            }
        }
        
        #endregion

        #region Elements Addition

        private void AddSearchWindow()
        {
            _searchWindow = ScriptableObject.CreateInstance<WUI_SearchWindow>();
            _searchWindow.Initialize(this);

            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        private void AddMiniMap()
        {
            (_miniMap as WUI_Graph_MiniMap)?.Initialize(
                isAnchored:true,
                rect:new Rect(15, 75, 200, 180),
                isVisible:false);

            _miniMap.AddToElement(this);
        }
        
        private void AddGridBackground()
        {
            if (_gridBackground is not WUI_GridBackground background) return;
                
            background.StretchToParentSize();
            
            Insert(0, background.GetBackground());
        }

        private void AddStyles()
        {
            #region Graph Style

            this.AddStyleSheets(
                "WhiteUI/WUI_GraphViewStyles.uss",
                "WhiteUI/WUI_NodeStyles.uss",
                "WhiteUI/WUI_GroupStyles.uss");

            #endregion

            #region MiniMap Style

            _miniMap.SetStyle();

            #endregion
        }

        public void ToggleMiniMap()
        {
            _miniMap.ChangeVisibleState(!_miniMap.GetVisibleState());
        }

        #endregion

        #region Utilities

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            var worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= _editorWindow.position.position;
            }
            
            var localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(RemoveElement);
            
            _groups.Clear();
            _groups.Clear();
            _ungroupedNodes.Clear();

            NamesErrorAmount = 0;
        }

        #endregion
    }
}