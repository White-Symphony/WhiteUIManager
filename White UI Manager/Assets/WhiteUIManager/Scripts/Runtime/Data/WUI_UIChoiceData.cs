using System;
using UnityEngine;

namespace WUI.Runtime.Data
{
    using ScriptableObjects;
    
    [Serializable]
    public class WUI_UIChoiceData
    {
        [field:SerializeField] public string Text { get; set; }
        
        [field:SerializeField] public WUI_UI_SO ChoiceUI { get; set; }
    }
}
