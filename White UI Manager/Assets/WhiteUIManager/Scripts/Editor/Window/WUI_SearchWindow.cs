using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace WUI.Editor.Window
{
    using Enumerations;
    using Graph;
    
    public class WUI_SearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private WUI_GraphView _graphView;

        private Texture2D _firstUIIcon;
        private Texture2D _MiddleUIIcon;
        private Texture2D _LastUIIcon;
        private Texture2D _groupIcon;
        private Texture2D _timeIcon;
        
        public void Initialize(WUI_GraphView graphView)
        {
            _graphView = graphView;

            _firstUIIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WhiteUIManager/ART/Textures/Icons/Black_Home_Icon.png");
            _MiddleUIIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WhiteUIManager/ART/Textures/Icons/Black_Node_Icon.png");
            _LastUIIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WhiteUIManager/ART/Textures/Icons/Black_Node_Icon.png");
            _groupIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WhiteUIManager/ART/Textures/Icons/Black_Group_Icon.png");
            _timeIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WhiteUIManager/ART/Textures/Icons/Black_Time_Icon.png");

        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchTreeEntries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create UI")),
                new SearchTreeGroupEntry(new GUIContent("UI Node"), 1),
                new (new GUIContent("Home UI", _firstUIIcon))
                {
                    level = 2,
                    userData = WUI_NodeType.HomeUI
                },
                new (new GUIContent("Basic UI", _MiddleUIIcon))
                {
                    level = 2,
                    userData = WUI_NodeType.BasicUI
                },
                new (new GUIContent("Last UI", _LastUIIcon))
                {
                    level = 2,
                    userData = WUI_NodeType.LastUI
                },
                new SearchTreeGroupEntry(new GUIContent("UI Group"), 1),
                new (new GUIContent("Basic Group", _groupIcon))
                {
                    level = 2,
                    userData = new Group()
                },
                new SearchTreeGroupEntry(new GUIContent("Time"), 1),
                new (new GUIContent("Wait Time", _timeIcon))
                {
                    level = 2,
                    userData = WUI_NodeType.WaitTime
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var localMousePosition = _graphView.GetLocalMousePosition(context.screenMousePosition, true);
            
            switch (SearchTreeEntry.userData)
            {
                case WUI_NodeType.HomeUI:
                    var firsUINode = _graphView.CreateNode("Home UI", WUI_NodeType.HomeUI, localMousePosition);
                    
                    _graphView.AddElement(firsUINode);
                    
                    return true;
                
                case WUI_NodeType.BasicUI:
                    var middleUINode = _graphView.CreateNode("Basic UI", WUI_NodeType.BasicUI, localMousePosition);
                    
                    _graphView.AddElement(middleUINode);
                    
                    return true;
                
                case WUI_NodeType.LastUI:
                    var lastUINode = _graphView.CreateNode("Last UI", WUI_NodeType.LastUI, localMousePosition);
                    
                    _graphView.AddElement(lastUINode);
                    
                    return true;
                
                case WUI_NodeType.WaitTime:
                    var waitTimeNode = _graphView.CreateNode("Wait Time", WUI_NodeType.WaitTime, localMousePosition);
                    _graphView.AddElement(waitTimeNode);
                    
                    return true;
                
                case Group:
                    _graphView.CreateGroup("UI Group", localMousePosition);
                    
                    return true;
                
                default: return false;
            }
        }
    }
}
