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
        
        private readonly Button _saveButton;
        
        private readonly Button _miniMapButton;

        public static string GetFileName() => _fileName;
        
        public Toolbar GetToolBar() => _toolbar;
        
        public WUI_Toolbar(WUI_GraphView graphView)
        {
            _graphView = graphView;
            
            _toolbar = new Toolbar();

            _saveButton = WUI_ElementUtility.CreateButton("Save", Save);
            
            _saveButton.SetEnabled(false);
            
            var clearButton = WUI_ElementUtility.CreateButton("Clear", Clear);
            var resetButton = WUI_ElementUtility.CreateButton("Reset", ResetGraph);
            _miniMapButton = WUI_ElementUtility.CreateButton("Mini Map", ToggleMiniMap);
            
            _toolbar.Add(_saveButton);
            _toolbar.Add(clearButton);
            _toolbar.Add(resetButton);
            _toolbar.Add(_miniMapButton);

            _toolbar.AddStyleSheets("WhiteUI/WUI_ToolbarStyles.uss");
        }

        #region Toolbar Actions

        public void LoadToInputPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            
            SetNamesByPath(filePath);
            
            Clear();

            WUI_IOUtility.Initialize(_graphView, _fileName);
            WUI_IOUtility.Load();
        }

        private void Save()
        {
            WUI_IOUtility.Initialize(_graphView, _fileName);
            WUI_IOUtility.Save();
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
        
        private void ResetGraph()
        {
            Clear();

            if (string.IsNullOrEmpty(_filePath)) return;
            
            LoadToInputPath(_filePath);
        }

        #endregion

        #region Utilities

        public void SaveButtonEnableState(bool state) => _saveButton.SetEnabled(state);

        public static void UpdateFileName(string fileName) => _fileName = fileName;
        
        private void SetNamesByPath(string filePath)
        {
            _filePath = filePath;
            _fileName = Path.GetFileNameWithoutExtension(filePath);
        }

        #endregion
    }
}