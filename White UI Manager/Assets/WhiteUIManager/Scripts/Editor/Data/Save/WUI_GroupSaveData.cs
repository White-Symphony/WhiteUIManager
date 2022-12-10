using System;
using UnityEngine;

namespace WUI.Editor.Data.Save
{
    [Serializable]
    public class WUI_GroupSaveData
    {
        [field:SerializeField] public string ID { get; set; }
        [field:SerializeField] public string Name { get; set; }
        [field:SerializeField] public Vector2 Position { get; set; }
    }
}