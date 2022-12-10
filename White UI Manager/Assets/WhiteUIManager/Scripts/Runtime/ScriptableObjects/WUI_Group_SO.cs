using UnityEngine;

namespace WUI.Runtime.ScriptableObjects
{
    public class WUI_Group_SO : ScriptableObject
    {
        [field:SerializeField] public string GroupName { get; set; }

        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }
    }
}