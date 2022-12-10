using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using WUI.Editor.Window;
using WUI.Utilities;

namespace WUI.Editor.Data.Save
{
    [CreateAssetMenu(menuName = "WhiteUI/UI Graph", fileName = "new UI Graph", order = 2)]
    public class WUI_GraphSaveData_SO : ScriptableObject
    {
        [field:SerializeField] public string FileName { get; set; }
        
        public List<WUI_GroupSaveData> Groups { get; set; }
        
        public List<WUI_NodeSaveData> Nodes { get; set; }
        
        public List<string> OldGroupNames { get; set; }
        
        public List<string> oldUngroupedNodeNames { get;set; }
        
        public WUI_SerializableDictionary<string, List<string>> OldGroupedNames { get; set; }

        private void Awake() => EditorApplication.projectChanged += SetFileName;

        private void OnDestroy() => EditorApplication.projectChanged -= SetFileName;
        
        private void SetFileName() => FileName = name;

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Groups = new List<WUI_GroupSaveData>();
            Nodes = new List<WUI_NodeSaveData>();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var project = EditorUtility.InstanceIDToObject(instanceID) as WUI_GraphSaveData_SO;
            if (project == null) return false;
            
            WUI_EditorWindow.OpenWindow();
            WUI_EditorWindow.GetGraphView().LoadData(AssetDatabase.GetAssetPath(instanceID));
            return true;
        }
    }
}