using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;
using WUI.Editor.Utilities;
using WUI.Editor.Window;
using WUI.Runtime.ScriptableObjects;
using WUI.Utilities;

namespace WUI.Editor.Data.ScriptableObjects
{
    [CreateAssetMenu(menuName = "WhiteUI/UI Graph", fileName = "new UI Graph", order = 2)]
    public class WUI_Graph_SO : ScriptableObject
    {
        [field:SerializeField] public string FileName { get; set; }
        
        [field:SerializeField] public List<WUI_Group_SO> Groups { get; set; }
        
        [field:SerializeField] public List<WUI_Node_SO> Nodes { get; set; }

        private WUI_Graph_SO savedGraph;

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
            Nodes ??= new List<WUI_Node_SO>();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID)
        {
            var project = EditorUtility.InstanceIDToObject(instanceID) as WUI_Graph_SO;
            if (project == null) return false;

            WUI_EditorWindow.OpenWindow();
            WUI_EditorWindow.GetGraphView().LoadData(AssetDatabase.GetAssetPath(instanceID), instanceID.ToString());

            return true;
        }

        private void LoadData(object _) => OnOpenAsset(GetInstanceID());

        private void Awake() => EditorApplication.projectChanged += SetFileName;

        private void OnEnable() => CompilationPipeline.compilationFinished += LoadData;

        private void OnDisable() => CompilationPipeline.compilationFinished -= LoadData; 

        public void DestroyHandler()
        {
            EditorApplication.projectChanged -= SetFileName;

            WUI_EditorWindow.GetGraphView().ClearGraph();
        }

        private void SetFileName()
        {
            if (!AssetDatabase.Contains(this))
            {
                EditorApplication.projectChanged -= SetFileName;
                WUI_EditorWindow.GetGraphView().ClearGraph();
                return;
            }
            FileName = name;
            
            WUI_IOUtility.DirtyAsset(this);
        }
    }
}