using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Editor.Graph;

namespace WUI.Editor.Window
{
    using Utilities;
    
    public class WUI_EditorWindow : EditorWindow
    {
        private static WUI_GraphView _graphView;

        private static WUI_Toolbar _toolbar;

        [MenuItem("WhiteUI/White UI Editor")]
        public static void OpenWindow()
        {
            GetWindow<WUI_EditorWindow>("White UI Editor");

            CompilationPipeline.compilationStarted  += OnStartCompile;
            CompilationPipeline.compilationFinished += OnCompiled;
        }

        public void OnDestroy()
        {
            CompilationPipeline.compilationStarted  -= OnStartCompile;
            CompilationPipeline.compilationFinished -= OnCompiled;
        }

        public static WUI_GraphView GetGraphView() => _graphView;

        public WUI_Toolbar GetToolbar() => _toolbar;
        
        private static void OnStartCompile(object obj)
        {
            var fileName = WUI_Toolbar.GetFileName();
            
            PlayerPrefs.SetString("GraphName", fileName);
            
            WUI_IOUtility.Initialize(_graphView, fileName);
            WUI_IOUtility.Save();
        }
         
        private static void OnCompiled(object obj)
        {
            //var fileName = PlayerPrefs.GetString("GraphName");
            // 
            //WUI_IOUtility.Initialize(_graphView, fileName);
            //WUI_IOUtility.Load();
        }

        private void OnEnable() 
        {
            AddGraphView();
            
            AddToolBar();

            AddStyles();
        }

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