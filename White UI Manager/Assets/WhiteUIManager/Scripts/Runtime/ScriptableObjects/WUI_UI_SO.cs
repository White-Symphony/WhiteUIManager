using UnityEngine;
using WUI.Editor.Data.Save;
using WUI.Editor.Enumerations;

namespace WUI.Runtime.ScriptableObjects
{
    public class WUI_UI_SO : ScriptableObject
    {
        public string ID { get; set; }
        public string GroupID { get; set; }
        
        public Vector2 Position { get; set; }
        
        public string UIName { get; set; }
        
        public string UIInformation { get; set; }
        
        public WUI_UISaveData PreviousUI { get; set; }
        
        public WUI_UISaveData NextUI { get; set; }
        
        public WUI_NodeType NodeType { get; set; }
        
        
        
        public bool IsStartingNode { get; set; }

        public void Initialize(
            string groupID = "",
            string id = "",
            string uiName = "",
            string uiInformation = "",
            WUI_UISaveData previousUI = null,
            WUI_UISaveData nextUI = null,
            WUI_NodeType nodeType = WUI_NodeType.BasicUI,
            bool isStartingNode = false)
        {
            GroupID = groupID;

            ID = id;

            UIName = uiName;

            UIInformation = uiInformation;

            PreviousUI = previousUI;

            NextUI = nextUI;

            NodeType = nodeType;

            IsStartingNode = isStartingNode;
        }
    }
}