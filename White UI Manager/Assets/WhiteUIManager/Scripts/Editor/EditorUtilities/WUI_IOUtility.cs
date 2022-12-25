using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using WUI.Editor.Data.Save;
using WUI.Editor.Enumerations;
using WUI.Editor.Window;
using WUI.Runtime.ScriptableObjects;

namespace WUI.Editor.Utilities
{
    using Graph;
    using Elements;
    using Data.ScriptableObjects;

    public static class WUI_IOUtility
    {
        private static WUI_GraphView _graphView;

        private static string _containerFolderPath;

        private static Dictionary<string, WUI_Group> _groups;
        private static Dictionary<string, WUI_Node> _nodes;

        private static string _graphDataInstanceID;

        public static void Initialize(WUI_GraphView graphView)
        {
            _graphView = graphView;

            _groups = new Dictionary<string, WUI_Group>();

            _nodes = new Dictionary<string, WUI_Node>();
        }

        #region Add Methods

        public static void AddObjectToAsset(Object obj, Object asset)
        {
            AssetDatabase.AddObjectToAsset(obj, asset);

            AssetDatabase.SaveAssets();
        }

        public static void AddGroup(WUI_Group group)
        {
            var graphData = GetGraphData();

            var groupData = GetGroupByID(group.ID);

            if (groupData == null)
            {
                var g_data = GetGraphData();

                groupData = ScriptableObject.CreateInstance<WUI_Group_SO>();

                groupData.name = group.title;
                
                AddObjectToAsset(groupData, g_data);
                
                graphData.Groups.Add(groupData);
            }

            groupData.Initialize(group.ID, group.title, group.GetPosition().position);
            
            SaveAsset(groupData);
            SaveAsset(graphData);
        }
        
        public static void AddNode(WUI_Node node, Vector2 position)
        {
            var graphData = GetGraphData();

            var nodeData = GetNodeByID(node.ID);

            if (nodeData == null)
            {
                var g_data = GetGraphData();

                nodeData = ScriptableObject.CreateInstance<WUI_Node_SO>();

                if (nodeData != null)
                {
                    nodeData.name = node.UIName;

                    AddObjectToAsset(nodeData, g_data);

                    graphData.Nodes.Add(nodeData);
                }
            }

            var groupID = "";

            if (node.Group != null) groupID = node.Group.ID;

            nodeData.Initialize(
                groupID,
                node.ID,
                node.UIName,
                position,
                node.PreviousNodes,
                node.NextNodes,
                node.NodeType,
                node.IsStartingNode());

            SaveAsset(nodeData);
            SaveAsset(graphData);
            
            _graphView.AddElement(node);
        }

        #endregion

        #region Load Methods

        public static void Load(WUI_Graph_SO graphData)
        {
            if (graphData == null) return;
            
            graphData.Initialize(graphData.FileName);

            WUI_Toolbar.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();

            WUI_EditorWindow.GetGraphView().SetViewPositionToObjectCenter(Vector3.zero);
        }
        
        public static void Load(string instanceID)
        {
            _graphDataInstanceID = instanceID;
            
            var graphData = GetGraphData();

            if (graphData == null) return;
            
            graphData.Initialize(graphData.FileName);

            WUI_Toolbar.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();

            WUI_EditorWindow.GetGraphView().SetViewPositionToObjectCenter(Vector3.zero);
        }

        #endregion

        #region Remove Methods

        public static void RemoveGroupByID(string groupID)
        {
            var groupData = GetGroupByID(groupID);

            var graphData = GetGraphData();
            
            if (!graphData.RemoveGroup(groupID)) return;
            
            AssetDatabase.RemoveObjectFromAsset(groupData);
            SaveAsset(groupData);
            SaveAsset(graphData);
        }
        
        public static void RemoveNodeByID(string nodeID)
        {
            var nodeData = GetNodeByID(nodeID);

            var graphData = GetGraphData();
            
            if (!graphData.RemoveNode(nodeID)) return;
            
            AssetDatabase.RemoveObjectFromAsset(nodeData);
            
            SaveAsset(nodeData);
            SaveAsset(graphData);
        }

        #endregion

        #region Graph

        public static WUI_Graph_SO GetGraphData()
        {
            var path = AssetDatabase.GetAssetPath(int.Parse(_graphDataInstanceID));
            
            return AssetDatabase.LoadAssetAtPath<WUI_Graph_SO>(path);
        }

        #endregion
        
        #region Groups

        private static void LoadGroups(List<WUI_Group_SO> groups)
        {
            if (groups.Count < 1) return;

            var cloneGroups = groups.ToList();
            
            foreach (var groupData in cloneGroups)
            {
                if (_graphView.AddGroup(groupData) is not WUI_Group group) continue;

                group.ID = groupData.ID;
                group.title = groupData.Name;

                group.SetPosition(new Rect(groupData.Position.x, groupData.Position.y, 100, 100));

                _groups.Add(group.ID, group);
            }
        }
        
        public static WUI_Group_SO GetGroupByID(string groupID)
        {
            var graphData = GetGraphData();

            return graphData.Groups.Count < 1 ?
                null : 
                graphData.Groups.FirstOrDefault(g => g.ID == groupID);
        }

        #endregion

        #region Node

        private static void LoadNodes(List<WUI_Node_SO> nodes)
        {
            if (nodes.Count < 1)
            {
                _graphView.CreateNode("Home UI", WUI_NodeType.HomeUI, Vector2.zero);
                return;
            }
            
            foreach (var nodeData in nodes)
            {
                if(_graphView.AddNode(nodeData) is not WUI_Node node) continue;

                node.ID = nodeData.ID;
                node.NextNodes = nodeData.NextNodes;
                node.PreviousNodes = nodeData.PreviousNodes;

                node.Draw();
                
                _graphView.AddElement(node);
                
                _nodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID)) continue;

                var group = _groups[nodeData.GroupID];

                node.Group = group;
                
                group.AddElement(node);
            }
        }
        
        public static WUI_Node_SO GetNodeByID(string nodeID)
        {
            var graphData = GetGraphData();

            return graphData.Nodes.Count < 1 ?
                null :
                graphData.Nodes.FirstOrDefault(n => n.ID == nodeID);
        }

        #endregion
        
        #region Connection

        public static void UpdateNodeConnection(WUI_Node_SO nodeData)
        {
            var previousNodesData = nodeData.PreviousNodes;
            var nextNodesData = nodeData.NextNodes;

            if (previousNodesData != null)
            {
                foreach (var previousNode_nextNode in previousNodesData.
                             Select(previousNodeData => GetNodeByID(previousNodeData.NodeID)).
                             Where(previousNode => previousNode != null).
                             Select(previousNode => previousNode.NextNodes).
                             SelectMany(previousNode_nextNodes => previousNode_nextNodes.
                                 Where(previousNode_nextNode => previousNode_nextNode.NodeID == nodeData.ID)))
                {
                    previousNode_nextNode.NodeName = nodeData.NodeName;
                }
            }
            
            if (nextNodesData != null)
            {
                foreach (var nextNode_previousNode in nextNodesData.
                             Select(nextNodeData => GetNodeByID(nextNodeData.NodeID)).
                             Where(nextNode => nextNode != null).Select(nextNode => nextNode.PreviousNodes).
                             SelectMany(nextNode_previousNodes => nextNode_previousNodes.
                                 Where(nextNode_previousNode => nextNode_previousNode.NodeID == nodeData.ID)))
                {
                    nextNode_previousNode.NodeName = nodeData.NodeName;
                }
            }

            DirtyAsset(nodeData);
        }
        
        public static void UpdateNodeConnection(string nodeID)
        {
            var nodeData = GetNodeByID(nodeID);

            if (nodeData == null) return;
            
            UpdateNodeConnection(nodeData);
        }

        private static void LoadNodesConnections()
        {
            if (_nodes.Count < 1) return;
            
            foreach (var node in _nodes)
            {
                var currentNode = node.Value;
                
                var outputPorts = currentNode.outputContainer.Children().Select(n => n as WUI_Port).ToList();
                var nextNodesData =currentNode.NextNodes;

                for (var i = 0; i < outputPorts.Count; i++)
                {
                    var outputPort = outputPorts[i];
                    var nextNodeData = nextNodesData[i];

                    if (nextNodeData == null || outputPort == null) continue;
                    
                    if(outputPort.connected) continue;

                    if (string.IsNullOrEmpty(nextNodeData.NodeID)) continue;

                    var nextNode = _nodes[nextNodeData.NodeID];

                    var index = nextNode.PreviousNodes.FindIndex(n => n.NodeID == currentNode.ID);
                    
                    if(index < 0) continue;

                    var nexttPort = nextNode.inputContainer[index];

                    var edge = outputPort.ConnectTo(nexttPort as WUI_Port);

                    if (edge == null) continue;
                    
                    edge.enableFlow = true;

                    _graphView.AddElement(edge);
                }

                node.Value.RefreshPorts();
            }
        }

        #endregion

        #region Utility Methods

        public static void DirtyAsset(Object asset) => EditorUtility.SetDirty(asset);

        public static void SaveAsset(Object asset)
        {
            EditorUtility.SetDirty(asset);
            
            AssetDatabase.SaveAssets();
        }

        #endregion
    }
}
