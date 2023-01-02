using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly WUI_IManipulator _manipulator;
        
        private readonly WUI_Graph_Feature _miniMap;
        private readonly WUI_Graph_Feature _gridBackground;

        private Label _noGraphsText;
        
        public int NamesErrorAmount { get; set; }

        public WUI_GraphView(WUI_EditorWindow editorWindow)
        {
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
            
            AddNoGraphsText();

            AddSearchWindow();

            AddMiniMap();

            OnElementsDeleted();

            OnGroupElementsAdded();

            OnGroupElementsRemoved();

            OnGroupRenamed();

            OnGraphViewChanged();
            
            AddStyles();
        }

        public void LoadData(string filePath, string instanceID)
        {
            RemoveNoGraphsText();
            AddGridBackground();
            
            _editorWindow.GetToolbar().LoadToInputPath(filePath, instanceID);
        }

        #region Override Methods

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (startPort is WUI_Port port) port.OnSetupPorts(startPort);

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
            _manipulator.SetManipulator(this);
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, Action<DropdownMenuAction> action)
        {
            var contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => action?.Invoke(actionEvent)));

            return contextualMenuManipulator;
        }

        #endregion

        #region Elements Creation

        public WUI_Group CreateGroup(string title, Vector2 mousePosition)
        {
            var id = Guid.NewGuid().ToString();
            
            var group = new WUI_Group(id, title, mousePosition);
            
            AddGroup(group);
            
            AddElement(group);

            #region Automatical Nodes Addiction

            foreach (var selectedElement in selection)
            {
                if (selectedElement is not WUI_Node node) continue;
                
                group.AddElement(node);
            }

            #endregion
            
            WUI_IOUtility.AddGroup(group);

            return group;
        }

        public Group AddGroup(WUI_Group_SO groupSO)
        {
            var groupData = WUI_IOUtility.GetGroupByID(groupSO.ID);

            if (groupData == null) return null;

            var group = new WUI_Group(groupSO.ID, groupSO.Name, groupSO.Position);

            AddGroup(group);
            
            AddElement(group);

            WUI_IOUtility.AddGroup(group);

            return group;
        }

        public Node AddNode(WUI_Node_SO nodeSo)
        {
            var type = Type.GetType($"WUI.Editor.Elements.WUI_{nodeSo.NodeType}_Node");

            if (type == null) return null;

            var nodeData = WUI_IOUtility.GetNodeByID(nodeSo.ID);

            if (nodeData == null) return null;
            
            if (Activator.CreateInstance(type) is not WUI_Node node) return null;

            node.Initialize(nodeData, this, nodeSo.NodeType);

            AddUngroupedNode(node);
            
            AddElement(node);

            return node;
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
            
            WUI_IOUtility.AddNode(node, position);

            return node;
        }

        #endregion

        #region Callbacks

        private void OnElementsDeleted()
        {
            deleteSelection = (_, _) =>
            {
                var groupType = typeof(WUI_Group);

                var edgeType = typeof(WUI_Edge);
                
                var groupsToDelete = new List<WUI_Group>();

                var edgesToDelete = new List<WUI_Edge>();
                
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
                        var edge = element as WUI_Edge;
                        
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
                    
                    WUI_IOUtility.RemoveGroupByID(group.ID);
                }

                DeleteElements(edgesToDelete);
                
                foreach (var node in nodesToDelete)
                {
                    node.Group?.RemoveElement(node);

                    RemoveUngroupedNode(node);
                    
                    node.DisconnectAllPorts();

                    RemoveElement(node);

                    WUI_IOUtility.RemoveNodeByID(node.ID);
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

                var groupData = WUI_IOUtility.GetGroupByID(wui_group.ID);

                groupData.Name = wui_group.title;

                WUI_IOUtility.DirtyAsset(groupData);
                
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
                if (changes.movedElements != null)
                {
                    foreach (var movedElement in changes.movedElements)
                    {
                        switch (movedElement)
                        {
                            case WUI_Node node:
                            {
                                var nodeData = WUI_IOUtility.GetNodeByID(node.ID);
                        
                                if(nodeData == null) continue;

                                nodeData.Position = node.GetPosition().position;
                                break;
                            }
                            case WUI_Group group:
                                var groupData = WUI_IOUtility.GetGroupByID(group.ID);

                                if (groupData == null) continue;

                                groupData.Position = group.GetPosition().position;

                                WUI_IOUtility.DirtyAsset(groupData);
                                
                                foreach (var node in nodes.Select(n => n as WUI_Node))
                                {
                                    if (node == null) continue;
                                    
                                    var nodeData = WUI_IOUtility.GetNodeByID(node.ID);

                                    if (nodeData == null) continue;

                                    nodeData.Position = node.GetPosition().position;
                                    
                                    WUI_IOUtility.DirtyAsset(nodeData);
                                }
                                
                                break;
                        }
                    }
                }

                if (changes.edgesToCreate != null)
                {
                    foreach (var edge in changes.edgesToCreate)
                    {
                        var inputPort = edge.input as WUI_Port;
                        var outputPort = edge.output as WUI_Port;

                        inputPort.OnRemovePorts(inputPort);
                        outputPort.OnRemovePorts(outputPort);

                        var previousNode = edge.output.node as WUI_Node;
                        var nextNode = edge.input.node as WUI_Node;

                        if (edge.input.userData is WUI_NodeData nextNodeData)
                        {
                            nextNodeData.NodeID = previousNode.ID;
                            nextNodeData.NodeName = previousNode.UIName;
                        }

                        if (edge.output.userData is WUI_NodeData previousNodeData)
                        {
                            previousNodeData.NodeID = nextNode.ID;
                            previousNodeData.NodeName = nextNode.UIName;
                        }

                        var whiteEdge = edge as WUI_Edge;

                        whiteEdge?.EdgeCheckConnection();
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    foreach (var element in changes.elementsToRemove.Where(element => element.GetType() == typeof(WUI_Edge)))
                    {
                        if(element is not WUI_Edge edge) continue;

                        if (edge.output.userData is WUI_NodeData outputUIData)
                        {
                            outputUIData.NodeName = "";
                            outputUIData.NodeID = "";
                        }
                        
                        if (edge.input.userData is WUI_NodeData inputUIData)
                        {
                            inputUIData.NodeName = "";
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

            WUI_IOUtility.GetNodeByID(node.ID).GroupID = node.Group.ID;
            
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

            var nodeData = WUI_IOUtility.GetNodeByID(node.ID);
            nodeData.GroupID = "";
            
            WUI_IOUtility.DirtyAsset(nodeData);
            
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

        public void AddNoGraphsText()
        {
            _noGraphsText = new Label
            {
                text = "No Graphs",
                style =
                {
                    fontSize = 100,
                    paddingLeft = viewport.transform.position.x / 2,
                    paddingTop = viewport.transform.position.y / 2,
                    backgroundColor = new Color(0.12f, 0.12f, 0.12f),
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            
            _noGraphsText.StretchToParentSize();
            
            Insert(0, _noGraphsText);
        }
        
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

        public void AddGridBackground()
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

        #region Elements Remove

        public void RemoveNoGraphsText()
        {
            if (_noGraphsText.parent != this) return;
            
            Remove(_noGraphsText);
        }
        
        public void RemoveGridBackground()
        {
            if (_gridBackground is not WUI_GridBackground background) return;

            if (background.GetBackground().parent != this) return;
            
            Remove(background.GetBackground());
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

        public void SetViewPositionToObjectCenter(Vector3 position)
        {
            var windowCenter = new Vector3(_editorWindow.position.width / 2, _editorWindow.position.height / 2);

            viewTransform.position = position + windowCenter;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(RemoveElement);
            
            _groups.Clear();
            _groups.Clear();
            _ungroupedNodes.Clear();

            NamesErrorAmount = 0;
            
            RemoveGridBackground();
            AddNoGraphsText();
        }

        #endregion
    }
}