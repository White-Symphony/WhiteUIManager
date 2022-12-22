using System.Linq;
using UnityEngine;

namespace WUI.Global.Database
{
    [CreateAssetMenu(menuName = "WhiteUI/Data/WhiteData")]
    public class WUI_WhiteDatabase_SO : ScriptableObject 
    {
        public WUI_TextureDatabase TextureDatabase;
    }

    public static class WUI_WhiteDatabase
    {
        private static WUI_WhiteDatabase_SO _database;
        
        private static WUI_WhiteDatabase_SO GetDatabase()
        {
            return _database ??= Resources.LoadAll<WUI_WhiteDatabase_SO>("").First();
        }

        private static WUI_TextureDatabase _textureDatabase;

        public static WUI_TextureDatabase TextureDatabase
        {
            get
            {
                _textureDatabase ??= GetDatabase().TextureDatabase;
                
                return _textureDatabase;
            }
        }
    }
}