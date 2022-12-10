using System.Collections.Generic;
using UnityEngine;

namespace WUI.Runtime.ScriptableObjects
{
    using Utilities;
    
    public class WUI_UIContainer_SO : ScriptableObject
    {
        [field:SerializeField] public string FileName { get; set; }
        
        [field:SerializeField] public WUI_SerializableDictionary<WUI_Group_SO, List<WUI_UI_SO>> UIGroups { get; set; }
        
        [field:SerializeField] public List<WUI_UI_SO> UngroupedUIs { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            UIGroups = new WUI_SerializableDictionary<WUI_Group_SO, List<WUI_UI_SO>>();
            UngroupedUIs = new List<WUI_UI_SO>();
        }
    }
}