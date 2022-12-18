using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace WUI.Editor.Window
{
    using Graph;
    using Utilities;
    
    public class WUI_Toolbar : MonoBehaviour
    {
        private static WUI_GraphView _graphView;
        
        private readonly Toolbar _toolbar;
        
        private string _filePath;
        private static string _fileName;

        private readonly Button _miniMapButton;

        public static string GetFileName() => _fileName;
        
        public Toolbar GetToolBar() => _toolbar;
        
        public WUI_Toolbar(WUI_GraphView graphView)
        {
            _graphView = graphView;
            
            _toolbar = new Toolbar();

            var clearButton = WUI_ElementUtility.CreateButton("Clear", Clear);
            _miniMapButton = WUI_ElementUtility.CreateButton("Mini Map", ToggleMiniMap);
            
            _toolbar.Add(clearButton);
            _toolbar.Add(_miniMapButton);

            _toolbar.AddStyleSheets("WhiteUI/WUI_ToolbarStyles.uss");
        }

        #region Toolbar Actions

        public void LoadToInputPath(string filePath, string instanceID)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            SetNamesByPath(filePath);
            
            Clear();

            WUI_IOUtility.Initialize(_graphView, _fileName);
            WUI_IOUtility.Load(instanceID);
        }

        private static void Clear()
        {
            _graphView.ClearGraph();
        }

        private void ToggleMiniMap()
        {
            _graphView.ToggleMiniMap();
            
            _miniMapButton.ToggleInClassList("wui-toolbar__button_selected");
        }

        #endregion

        #region Utilities

        public static void UpdateFileName(string fileName) => _fileName = fileName;
        
        private void SetNamesByPath(string filePath)
        {
            _filePath = filePath;
            _fileName = Path.GetFileNameWithoutExtension(filePath);
        }

        #endregion
    }
}