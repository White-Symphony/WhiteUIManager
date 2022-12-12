using UnityEngine;
using WUI.Editor.Data.Save;
using WUI.Editor.Enumerations;

namespace WUI.Runtime.ScriptableObjects
{
    public class WUI_UI_SO : ScriptableObject
    {
        public string UIName { get; set; }
        
        public string UIInformation { get; set; }
        
        public WUI_UISaveData PreviousUI { get; set; }
        
        public WUI_UISaveData NextUI { get; set; }
        
        public WUI_NodeType NodeType { get; set; }
        
        public bool IsStartingNode { get; set; }

        public void Initialize(string uiName, string uiInformation, WUI_UISaveData previousUI, WUI_UISaveData nextUI, WUI_NodeType nodeType, bool isStartingNode)
        {
            UIName = uiName;
            
            UIInformation = uiInformation;
            
            PreviousUI = previousUI;
            
            NextUI = nextUI;
            
            NodeType = nodeType;

            IsStartingNode = isStartingNode;
        }
    }
}