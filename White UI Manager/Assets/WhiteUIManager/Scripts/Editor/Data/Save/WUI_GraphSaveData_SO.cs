using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using WUI.Editor.Window;
using WUI.Runtime.ScriptableObjects;
using WUI.Utilities;

namespace WUI.Editor.Data.Save
{
    [CreateAssetMenu(menuName = "WhiteUI/UI Graph", fileName = "new UI Graph", order = 2)]
    public class WUI_GraphSaveData_SO : ScriptableObject
    {
        public string FileName { get; set; }
        
        public List<WUI_GroupSaveData> Groups { get; set; }
        
        [field:SerializeField] public List<WUI_UI_SO> Nodes { get; set; }
        
        public List<string> OldGroupNames { get; set; }
        
        public List<string> oldUngroupedNodeNames { get;set; }
        
        public WUI_SerializableDictionary<string, List<string>> OldGroupedNames { get; set; }

        private void Awake()
        {
            EditorApplication.projectChanged += SetFileName;
        }

        private void OnDestroy() => EditorApplication.projectChanged -= SetFileName;

        private void SetFileName()
        {
            if (!AssetDatabase.Contains(this))
            {
                EditorApplication.projectChanged -= SetFileName;
                return;
            }
            FileName = name;
        }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            
            Groups ??= new List<WUI_GroupSaveData>();
            Nodes ??= new List<WUI_UI_SO>();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line) 
        {
            var project = EditorUtility.InstanceIDToObject(instanceID) as WUI_GraphSaveData_SO;
            if (project == null) return false;

            WUI_EditorWindow.OpenWindow();
            WUI_EditorWindow.GetGraphView().LoadData(AssetDatabase.GetAssetPath(instanceID), instanceID.ToString());
            return true;
        }
    }
}