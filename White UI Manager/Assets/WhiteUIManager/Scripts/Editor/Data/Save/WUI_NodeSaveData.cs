using UnityEngine;
using System;

namespace WUI.Editor.Data.Save
{
    using Enumerations;

    [Serializable]
    public class WUI_NodeSaveData
    {
        [field:SerializeField] public string ID { get; set; }
        
        [field:SerializeField] public string Name { get; set; }
        
        [field:SerializeField] public string GroupID { get; set; }
        
        [field:SerializeField] public WUI_UISaveData PreviousUI { get; set; }
        
        [field:SerializeField] public WUI_UISaveData NextUI { get; set; }

        [field:SerializeField] public WUI_UIType UIType { get; set; }
        
        [field:SerializeField] public Vector2 Position { get; set; }
    }
}