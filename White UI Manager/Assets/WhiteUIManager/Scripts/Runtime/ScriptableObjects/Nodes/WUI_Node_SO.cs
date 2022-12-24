using System.Collections.Generic;
using UnityEngine;
using WUI.Editor.Data.Save;
using WUI.Editor.Enumerations;

namespace WUI.Runtime.ScriptableObjects
{
    [System.Serializable]
    public class WUI_Node_SO : ScriptableObject
    {
        [field:SerializeField] public string ID { get; set; }
        
        [field:SerializeField] public string GroupID { get; set; }
        
        [field:SerializeField] public Vector2 Position { get; set; }
        
        [field:SerializeField] public string NodeName { get; set; }

        [field:SerializeField] public List<WUI_NodeData> PreviousNodes { get; set; }
        
        [field:SerializeField] public List<WUI_NodeData> NextNodes { get; set; }
        
        [field:SerializeField] public WUI_NodeType NodeType { get; set; }

        public bool IsStartingNode { get; set; }

        public void Initialize(
            string groupID = "",
            string id = "",
            string uiName = "",
            Vector2 position = default,
            List<WUI_NodeData> previousNodes = null,
            List<WUI_NodeData> nextNodes = null,
            WUI_NodeType nodeType = WUI_NodeType.BasicUI,
            bool isStartingNode = false)
        {
            GroupID = groupID;

            ID = id;

            NodeName = uiName;

            Position = position;

            PreviousNodes = previousNodes;

            NextNodes = nextNodes;

            NodeType = nodeType;

            IsStartingNode = isStartingNode;
        }
    }
}