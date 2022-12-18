using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using WUI.Editor.Window;
using WUI.Runtime.ScriptableObjects;
using WUI.Utilities;

namespace WUI.Editor.Utilities
{
    using Graph;
    using Elements;
    using Data.Save;

    public static class WUI_IOUtility
    {
        private static WUI_GraphView _graphView;
        
        private static string _graphFileName;
        private static string _containerFolderPath;

        private static List<WUI_Group> _groups;
        private static List<WUI_Node> _nodes;

        private static Dictionary<string, WUI_Group_SO> _createdUIGroups;
        private static Dictionary<string, WUI_UI_SO> _createUIs;
        private static Dictionary<string, WUI_Group> _loadedGroups;
        private static Dictionary<string, WUI_Node> _loadedNodes;

        private static string _graphDataInstanceID;

        public static void Initialize(WUI_GraphView graphView, string graphName)
        {
            _graphView = graphView;
            
            _graphFileName = graphName;

            _groups = new List<WUI_Group>();
            _nodes = new List<WUI_Node>();

            _createdUIGroups = new Dictionary<string, WUI_Group_SO>();
            _createUIs = new Dictionary<string, WUI_UI_SO>();

            _loadedGroups = new Dictionary<string, WUI_Group>();

            _loadedNodes = new Dictionary<string, WUI_Node>();
        }

        #region Save Methods

        public static void SaveNode(WUI_Node node)
        {
            var graphData = GetGraphData();

            var nodeSO = GetNodeByID(node.ID);

            if (nodeSO == null)
            {
                var g_data = LoadGraphData();
                
                nodeSO = ScriptableObject.CreateInstance<WUI_UI_SO>();
                
                nodeSO.name = node.UIName;
                
                AssetDatabase.AddObjectToAsset(nodeSO, g_data);
                AssetDatabase.SaveAssets();
                
                graphData.Nodes.Add(nodeSO);
            }

            var groupID = "";

            if (node.Group != null) groupID = node.Group.ID;
            
            nodeSO.Initialize(
                groupID,
                node.ID,
                node.UIName,
                node.UIInformation,
                node.PreviousUI,
                node.NextUI,
                node.NodeType,
                node.IsStartingNode());

            _graphView.AddElement(node);
        }
        
        public static void Save()
        {
            CreateStaticFolders();

            GetElementsFromGraphView();

            var path = AssetDatabase.GetAssetPath(int.Parse(_graphDataInstanceID));
            
            var graphData  = AssetDatabase.LoadAssetAtPath<WUI_GraphSaveData_SO>(path);

            graphData.Initialize(_graphFileName);

            SaveGroups(graphData);

            SaveNodes(graphData);
            
            SaveAsset(graphData);
        }

        #endregion

        #region Load Methods

        public static WUI_UI_SO GetNodeByName(string nodeName)
        {
            var graphData = GetGraphData();

            return graphData.Nodes.FirstOrDefault(n => n.UIName == nodeName);
        }
        
        public static WUI_UI_SO GetNodeByID(string nodeID)
        {
            var graphData = GetGraphData();

            return graphData.Nodes.Count < 1 ?
                null :
                graphData.Nodes.FirstOrDefault(n => n.ID == nodeID);
        }
        
        public static WUI_GraphSaveData_SO GetGraphData()
        {
            return LoadGraphData();
        }
        
        public static async void Load(string instanceID)
        {
            _graphDataInstanceID = instanceID;
            
            var graphData = LoadGraphData();

            if (graphData == null) return;
            
            graphData.Initialize(graphData.FileName);

            WUI_Toolbar.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadGroups(List<WUI_GroupSaveData> groups)
        {
            if (groups.Count < 1) return;
            
            foreach (var groupData in groups)
            {
                var group = _graphView.CreateGroup(groupData.Name, groupData.Position);

                group.ID = groupData.ID;
                
                _loadedGroups.Add(group.ID, group);
            }
        }

        public static WUI_UI_SO GetNodeSO(string nodeID)
        {
            var asset = LoadGraphData();

            return asset.Nodes.FirstOrDefault(node => node.ID == nodeID);
        }
        
        private static void LoadNodes(List<WUI_UI_SO> nodes)
        {
            if (nodes.Count < 1) return;
            
            foreach (var nodeData in nodes)
            {
                if(_graphView.CreateNode(nodeData.UIName, nodeData.NodeType, nodeData.Position, false) is not WUI_Node node) continue;

                node.ID = nodeData.ID;
                node.NextUI = nodeData.NextUI;
                node.PreviousUI = nodeData.PreviousUI;

                node.Draw();
                
                _graphView.AddElement(node);
                
                _loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID)) continue;

                var group = _loadedGroups[nodeData.GroupID];

                node.Group = group;
                
                group.AddElement(node);
            }
        }

        private static void LoadNodesConnections()
        {
            if (_loadedNodes.Count < 1) return;
            
            foreach (var loadedNode in _loadedNodes)
            {
                var outputPort = loadedNode.Value.outputContainer.Children().FirstOrDefault() as Port;
                var outputUIPort = loadedNode.Value.NextUI;

                if (outputUIPort != null && outputPort != null)
                {
                    if (string.IsNullOrEmpty(outputUIPort.NodeID)) continue;

                    var nextNode = _loadedNodes[outputUIPort.NodeID];

                    var nextNodeInputPort = nextNode.inputContainer.Children().FirstOrDefault() as Port;

                    var edge = outputPort.ConnectTo(nextNodeInputPort);
                    
                    _graphView.AddElement(edge);
                }
                
                var inputPort = loadedNode.Value.inputContainer.Children().FirstOrDefault() as Port;
                var inputUIPort = loadedNode.Value.PreviousUI;

                if (inputPort != null && inputUIPort != null)
                {
                    if (string.IsNullOrEmpty(inputUIPort.NodeID)) continue;

                    var nextNode = _loadedNodes[inputUIPort.NodeID];

                    var nextNodeInputPort = nextNode.outputContainer.Children().FirstOrDefault() as Port;

                    var edge = inputPort.ConnectTo(nextNodeInputPort);
                    
                    _graphView.AddElement(edge);
                }

                loadedNode.Value.RefreshPorts();
            }
        }
        
        #endregion
        
        #region Groups

        private static void SaveGroups(WUI_GraphSaveData_SO graphData)
        {
            var groupNames = new List<string>();
            
            foreach (var group in _groups)
            {
                SaveGroupToGraph(group, graphData);
                //SaveGroupToScriptableObject(group, uiContainer);
                
                groupNames.Add(group.title);
            }
            
            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToScriptableObject(WUI_Group group, WUI_UIContainer_SO uiContainer)
        {
            var groupName = group.title;
            
            CreateFolder($"{_containerFolderPath}/Groups", groupName);
            CreateFolder($"{_containerFolderPath}/Groups/{groupName}", "UIs");

            var uiGroup = CreateAsset<WUI_Group_SO>($"{_containerFolderPath}/Groups/{groupName}", groupName);
            
            uiGroup.Initialize(groupName);
            
            _createdUIGroups.Add(group.ID, uiGroup);
            
            uiContainer.UIGroups.Add(uiGroup, new List<WUI_UI_SO>());

            SaveAsset(uiGroup);
        }

        private static void SaveGroupToGraph(WUI_Group group, WUI_GraphSaveData_SO graphData)
        {
            var groupData = new WUI_GroupSaveData
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };
            
            graphData.Groups.Add(groupData);
        }
        
        private static void UpdateOldGroups(List<string> currentGroupNames, WUI_GraphSaveData_SO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                var groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (var groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{_containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            if (currentGroupNames != null) graphData.OldGroupNames = new List<string>(currentGroupNames);
        }

        #endregion

        #region Nodes

        private static void SaveNodes(WUI_GraphSaveData_SO graphData)
        {
            var groupedNodeNames = new WUI_SerializableDictionary<string, List<string>>();
            
            var ungroupedNodeNames = new List<string>();
            
            foreach (var node in _nodes)
            {
                SaveNode(node);

                if (node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.UIName);
                    
                    continue;
                }
                
                ungroupedNodeNames.Add(node.UIName);
            }

            UpdateUIConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);

            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void UpdateUIConnections()
        {
            foreach (var node in _nodes)
            {
                var ui = _createUIs.FirstOrDefault(p => p.Key == node.ID).Value;
                
                if(ui == null) continue;

                var previousUI = ui.PreviousUI;
                var nextUI = ui.NextUI;

                previousUI.Text = _createUIs[node.ID].PreviousUI.Text;
                previousUI.NodeID = _createUIs[node.ID].PreviousUI.NodeID;

                nextUI.Text = _createUIs[node.ID].NextUI.Text;
                nextUI.NodeID = _createUIs[node.ID].NextUI.NodeID;
                
                SaveAsset(ui);
            }
        }
        
        private static void UpdateOldGroupedNodes(WUI_SerializableDictionary<string,List<string>> currentGroupedNodeNames, WUI_GraphSaveData_SO graphData)
        {
            if (graphData.OldGroupedNames != null && graphData.OldGroupedNames.Count != 0)
            {
                foreach (var oldGroupedNode in graphData.OldGroupedNames)
                {
                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        var nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();

                        foreach (var nodeToRemove in nodesToRemove)
                        {
                            RemoveAsset($"{_containerFolderPath}/Groups/{oldGroupedNode.Key}/UIs", nodeToRemove);
                        }
                    }
                }
            }

            graphData.OldGroupedNames = new WUI_SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }
        
        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, WUI_GraphSaveData_SO graphData)
        {
            if (graphData.oldUngroupedNodeNames != null && graphData.oldUngroupedNodeNames.Count != 0)
            {
                var nodesToRemove = graphData.oldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach (var nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{_containerFolderPath}/Global/UIs", nodeToRemove);
                }
            }

            graphData.oldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        #endregion

        #region Creation Methods

        private static void CreateStaticFolders()
        {
            CreateFolder("Assets/WhiteUIManager", "Graphs");
            
            CreateFolder("Assets", "WhiteUIManager");
        }

        #endregion
        
        #region Fetch Methods

        private static void GetElementsFromGraphView()
        {
            var groupType = typeof(WUI_Group);
            
            _graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is WUI_Node node)
                {
                    _nodes.Add(node);

                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    var group = graphElement as WUI_Group;
                    
                    _groups.Add(group);
                }
            });
        }

        #endregion

        #region Utility Methods

        private static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}")) return;

            AssetDatabase.CreateFolder(path, folderName);
        }
        
        private static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }
        
        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            var fullPath = $"{path}";

            var asset = LoadAsset<T>(path, assetName);

            if (asset != null) return asset;
            
            asset = ScriptableObject.CreateInstance<T>();
            
            AssetDatabase.CreateAsset(asset, fullPath);

            return asset;
        }

        private static WUI_GraphSaveData_SO LoadGraphData()
        {
            var path = AssetDatabase.GetAssetPath(int.Parse(_graphDataInstanceID));
            
            return AssetDatabase.LoadAssetAtPath<WUI_GraphSaveData_SO>(path);
        }
        
        private static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            var fullPath = $"{path}/{assetName}.asset";
            
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }
        
        public static void SaveAsset(Object asset)
        {
            EditorUtility.SetDirty(asset);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        #endregion
    }
}
