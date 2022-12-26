using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using WUI.Editor.Data.Save;
using WUI.Editor.Elements;
using WUI.Editor.Graph;

namespace WUI.Editor.Window
{
    using Utilities;
    
    public class WUI_EditorWindow : EditorWindow
    {
        private static WUI_GraphView _graphView;

        private static WUI_Toolbar _toolbar;
        
        public static void OpenWindow()
        {
            GetWindow<WUI_EditorWindow>("White UI Editor");
        }

        private void Update()
        {
            UpdateEdge();
        }

        public static WUI_GraphView GetGraphView() => _graphView;

        public WUI_Toolbar GetToolbar() => _toolbar;

        private void OnEnable() 
        {
            AddGraphView();
            
            AddToolBar();

            AddStyles();
        }

        #region Edge

        private void UpdateEdge()
        {
            if (!EditorApplication.isPlaying)
            {
                foreach (var edge in _graphView.edges.Select(e => e as WUI_Edge).Where(e => e != null))
                {
                    edge.enableFlow = false;
                }
                
                return;
            }

            var nodes = _graphView.nodes.Select(e => e as WUI_Node).ToArray();

            foreach (var node in nodes)
            {
                if (node == null) continue;

                if(!node.IsStartingNode()) continue;
                
                var ports = node.outputContainer.Children().Select(e => e as WUI_Port).ToArray();

                foreach (var port in ports)
                {
                    var edges = port.connections.Select(e => e as WUI_Edge);
                    foreach (var edge in edges)
                    {
                        edge?.UpdateFlow();
                    }
                }

                if (node.NextNodes == null) continue;

                UpdateFlow(node.NextNodes, nodes);
            }
        }

        private void UpdateFlow(List<WUI_NodeData> nextNodes, WUI_Node[] nodes)
        {
            foreach (var nextNode in nextNodes)
            {
                if (nextNode == null) continue;
                    
                var selectedNode = nodes.FirstOrDefault(n => n.ID == nextNode.NodeID);

                if (selectedNode == null) continue;
                
                if (selectedNode.outputContainer.childCount < 1) continue;
                
                var selectedPorts = selectedNode.outputContainer.Children().Select(e => e as WUI_Port).ToArray();

                foreach (var port in selectedPorts)
                {
                    var edges = port.connections.Select(e => e as WUI_Edge);
                    
                    foreach (var edge in edges) edge?.UpdateFlow();
                }

                if (selectedNode.NextNodes == null) continue;
                
                UpdateFlow(selectedNode.NextNodes, nodes);
            }
        }

        #endregion
        
        #region Element Addition

        private void AddGraphView()
        {
            _graphView = new WUI_GraphView(this);
            
            _graphView.StretchToParentSize();
            
            rootVisualElement.Add(_graphView);
        }

        private void AddToolBar()
        {
            _toolbar = new WUI_Toolbar(_graphView);

            rootVisualElement.Add(_toolbar.GetToolBar());
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("WhiteUI/WUI_Variables.uss");
        }

        #endregion

        #region Utility Methods

        #endregion
    }
}