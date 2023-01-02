using UnityEditor;

namespace WUI.Editor.Data.ScriptableObjects
{
    public class WUI_Graph_SO_AssetProcessor: AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var assetPath in deletedAssets)
            {
                var graphData = AssetDatabase.LoadAssetAtPath<WUI_Graph_SO>(assetPath);

                if (graphData == null) continue;

                graphData.DestroyHandler();
            }
        }
    }
}