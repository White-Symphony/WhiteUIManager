using UnityEditor;
using WUI.Utilities;

namespace WUI.Editor.Data.ScriptableObjects
{
    [CustomEditor(typeof(WUI_Graph_SO))]
    public class WUI_Graph_SO_Editor : UnityEditor.Editor
    {
        private WUI_Graph_SO _graphData;

        private void OnEnable() => _graphData = target as WUI_Graph_SO;

        public override void OnInspectorGUI()
        {
            WUI_EditorUtilities.TitleWithIcon_Gray("Graph", "Graph");
            
            WUI_EditorUtilities.LabelFieldAndText("Graph Name", _graphData.FileName, out _);
        }
    }
}