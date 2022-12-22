using UnityEngine;

namespace WUI.Runtime.ScriptableObjects
{
    public class WUI_Group_SO : ScriptableObject
    {
        
        public string ID { get; set; }
        
        [field:SerializeField] public string Name { get; set; }
        
        public Vector2 Position { get; set; }

        public void Initialize(string id, string title, Vector2 position)
        {
            ID = id;
            Name = title;
            Position = position;
        }
    }
}