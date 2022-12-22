using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using WUI.Editor.Window;
using WUI.Runtime.ScriptableObjects;
using WUI.Utilities;

namespace WUI.Editor.Data.ScriptableObjects
{
    [CreateAssetMenu(menuName = "WhiteUI/UI Graph", fileName = "new UI Graph", order = 2)]
    public class WUI_Graph_SO : ScriptableObject
    {
        public string FileName { get; set; }
        
        [field:SerializeField] public List<WUI_Group_SO> Groups { get; set; }
        
        [field:SerializeField] public List<WUI_UI_SO> Nodes { get; set; }
        
        public List<string> OldGroupNames { get; set; }
        
        public List<string> oldUngroupedNodeNames { get;set; }
        
        public WUI_SerializableDictionary<string, List<string>> OldGroupedNames { get; set; }

        public bool RemoveGroup(string groupID)
        {
            var removingGroup = Groups.FirstOrDefault(g => g.ID == groupID);

            if (removingGroup == null) return false;

            Groups.Remove(removingGroup);

            return true;
        }
        
        public bool RemoveNode(string nodeID)
        {
            var removingNode = Nodes.FirstOrDefault(n => n.ID == nodeID);

            if (removingNode == null) return false;

            Nodes.Remove(removingNode);

            return true;
        }
        
        public void Initialize(string fileName)
        {
            FileName = fileName;
            
            Groups ??= new List<WUI_Group_SO>();
            Nodes ??= new List<WUI_UI_SO>();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line) 
        {
            var project = EditorUtility.InstanceIDToObject(instanceID) as WUI_Graph_SO;
            if (project == null) return false;

            WUI_EditorWindow.OpenWindow();
            WUI_EditorWindow.GetGraphView().LoadData(AssetDatabase.GetAssetPath(instanceID), instanceID.ToString());
            return true;
        }
        
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
    }
}