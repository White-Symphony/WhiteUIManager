using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using WUI.Utilities;

namespace WUI.Editor.Window
{
    using Enumerations;
    using Graph;
    
    public class WUI_SearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private WUI_GraphView _graphView;

        public void Initialize(WUI_GraphView graphView) => _graphView = graphView;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchTreeEntries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create UI")),
                new SearchTreeGroupEntry(new GUIContent("UI Node"), 1),
                new (new GUIContent("Basic UI", WUI_EditorUtilities.GetBlackIcon("Node")))
                {
                    level = 2,
                    userData = WUI_NodeType.BasicUI
                },
                new (new GUIContent("Last UI", WUI_EditorUtilities.GetBlackIcon("Node")))
                {
                    level = 2,
                    userData = WUI_NodeType.LastUI
                },
                new SearchTreeGroupEntry(new GUIContent("UI Group"), 1),
                new (new GUIContent("Basic Group", WUI_EditorUtilities.GetBlackIcon("Group")))
                {
                    level = 2,
                    userData = new Group()
                },
                new SearchTreeGroupEntry(new GUIContent("Time"), 1),
                new (new GUIContent("Wait Time", WUI_EditorUtilities.GetBlackIcon("Clock")))
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
