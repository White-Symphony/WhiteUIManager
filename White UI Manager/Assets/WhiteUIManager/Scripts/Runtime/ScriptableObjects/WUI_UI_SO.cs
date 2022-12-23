using UnityEngine;
using WUI.Editor.Data.Save;
using WUI.Editor.Enumerations;

namespace WUI.Runtime.ScriptableObjects
{
    [System.Serializable]
    public class WUI_UI_SO : ScriptableObject
    {
        [field:SerializeField] public string ID { get; set; }
        [field:SerializeField] public string GroupID { get; set; }
        
        [field:SerializeField] public Vector2 Position { get; set; }
        
        [field:SerializeField] public string UIName { get; set; }

        [field:SerializeField] public WUI_UISaveData PreviousUI { get; set; }
        
        [field:SerializeField] public WUI_UISaveData NextUI { get; set; }
        
        [field:SerializeField] public WUI_NodeType NodeType { get; set; }

        public bool IsStartingNode { get; set; }

        public void Initialize(
            string groupID = "",
            string id = "",
            string uiName = "",
            Vector2 position = default,
            WUI_UISaveData previousUI = null,
            WUI_UISaveData nextUI = null,
            WUI_NodeType nodeType = WUI_NodeType.BasicUI,
            bool isStartingNode = false)
        {
            GroupID = groupID;

            ID = id;

            UIName = uiName;

            Position = position;

            PreviousUI = previousUI;

            NextUI = nextUI;

            NodeType = nodeType;

            IsStartingNode = isStartingNode;
        }
    }
}