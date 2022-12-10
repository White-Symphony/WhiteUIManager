using System;
using UnityEngine;

namespace WUI.Editor.Data.Save
{
    [Serializable]
    public class WUI_UISaveData
    {
        [field:SerializeField] public string Text { get; set; }
        
        [field:SerializeField] public string NodeID { get; set; }
    }
}