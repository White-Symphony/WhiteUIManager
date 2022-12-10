using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace WUI.Editor.Window
{
    using Enumerations;
    using Graph;
    
    public class WUI_SearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private WUI_GraphView _graphView;

        private Texture2D indentationIcon;
        
        public void Initialize(WUI_GraphView graphView)
        {
            _graphView = graphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.white);
            indentationIcon.Apply();
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchTreeEntries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create UI")),
                new SearchTreeGroupEntry(new GUIContent("UI Node"), 1),
                new (new GUIContent("First UI", indentationIcon))
                {
                    level = 2,
                    userData = WUI_UIType.FirstUI
                },
                new (new GUIContent("Middle UI", indentationIcon))
                {
                    level = 2,
                    userData = WUI_UIType.MiddleUI
                },
                new (new GUIContent("Last UI", indentationIcon))
                {
                    level = 2,
                    userData = WUI_UIType.LastUI
                },
                new SearchTreeGroupEntry(new GUIContent("UI Group"), 1),
                new (new GUIContent("Basic Group", indentationIcon))
                {
                    level = 2,
                    userData = new Group()
                },
                new SearchTreeGroupEntry(new GUIContent("Time"), 1),
                new (new GUIContent("Wait Time", indentationIcon))
                {
                    level = 2,
                    userData = WUI_UIType.WaitTime
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var localMousePosition = _graphView.GetLocalMousePosition(context.screenMousePosition, true);
            
            switch (SearchTreeEntry.userData)
            {
                case WUI_UIType.FirstUI:
                    var firsUINode = _graphView.CreateNode("First UI", WUI_UIType.FirstUI, localMousePosition);
                    
                    _graphView.AddElement(firsUINode);
                    
                    return true;
                
                case WUI_UIType.MiddleUI:
                    var middleUINode = _graphView.CreateNode("Middle UI", WUI_UIType.MiddleUI, localMousePosition);
                    
                    _graphView.AddElement(middleUINode);
                    
                    return true;
                
                case WUI_UIType.LastUI:
                    var lastUINode = _graphView.CreateNode("Last UI", WUI_UIType.LastUI, localMousePosition);
                    
                    _graphView.AddElement(lastUINode);
                    
                    return true;
                
                case WUI_UIType.WaitTime:
                    var waitTimeNode = _graphView.CreateNode("Wait Time", WUI_UIType.WaitTime, localMousePosition);
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
