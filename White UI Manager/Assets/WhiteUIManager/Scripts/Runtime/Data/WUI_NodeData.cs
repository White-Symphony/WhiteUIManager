using System;
using UnityEngine;

namespace WUI.Editor.Data.Save
{
    [Serializable]
    public class WUI_NodeData
    {
        [field:SerializeField] public string NodeName { get; set; }
        
        [field:SerializeField] public string NodeID { get; set; }
    }
}