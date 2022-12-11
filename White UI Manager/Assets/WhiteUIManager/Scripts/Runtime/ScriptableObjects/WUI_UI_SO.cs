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
        
        public WUI_UIType UIType { get; set; }
        
        public bool IsStartingNode { get; set; }

        public void Initialize(string uiName, string uiInformation, WUI_UISaveData previousUI, WUI_UISaveData nextUI, WUI_UIType uiType, bool isStartingNode)
        {
            UIName = uiName;
            
            UIInformation = uiInformation;
            
            PreviousUI = previousUI;
            
            NextUI = nextUI;
            
            UIType = uiType;

            IsStartingNode = isStartingNode;
        }
    }
}