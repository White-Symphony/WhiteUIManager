using UnityEditor;
using WUI.Utilities;

namespace WUI.Runtime.ScriptableObjects
{
    [CustomEditor(typeof(WUI_Group_SO))]
    public class WUI_Group_SO_Editor : UnityEditor.Editor
    {
        private WUI_Group_SO _groupData;

        private void OnEnable() => _groupData = target as WUI_Group_SO;

        public override void OnInspectorGUI()
        {
            #region Group Title

            WUI_EditorUtilities.TitleWithIcon_Red("Group",  "Group Node");
            
            #endregion

            #region Group Name

            WUI_EditorUtilities.LabelFieldAndText("Group Name", _groupData.Name, out var newName);
            _groupData.Name = newName;

            #endregion

        }
    }
}