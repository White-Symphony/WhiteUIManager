using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WUI.Global.Database
{
    [CreateAssetMenu(menuName = "WhiteUI/Data/TextureDatabase")]
    public class WUI_TextureDatabase : ScriptableObject
    {
        [SerializeField] private List<WUI_TextureGroup> TextureGroups;

        public WUI_TextureGroup GetGroup(string groupName)
        {
            return TextureGroups.FirstOrDefault(g => g.Name == groupName);
        }

        public Texture2D GetTexture(string groupName, string textureName)
        {
            var group = GetGroup(groupName);

            return group.Textures.FirstOrDefault(t => t.Name == textureName).Texture;
        }
        
        [System.Serializable]
        public struct WUI_TextureGroup
        {
            public string Name;
            public List<WUI_TextureData> Textures;
        }
        
        [System.Serializable]
        public struct WUI_TextureData
        {
            public string Name;
            public Texture2D Texture;
        }
    }
}
