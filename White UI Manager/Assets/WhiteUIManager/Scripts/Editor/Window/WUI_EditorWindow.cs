using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
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
            UpdateEdgeFlow();
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

        private void UpdateEdgeFlow()
        {
            if (!EditorApplication.isPlaying)
            {
                foreach (var edge in _graphView.edges.Select(e => e as WUI_Edge))
                {
                    if (edge == null) continue;

                    edge.enableFlow = false;
                }
                
                return;
            }
            
            foreach (var edge in _graphView.edges.Select(e => e as WUI_Edge))
            {
                edge?.UpdateFlow();
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