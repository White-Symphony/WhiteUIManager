using UnityEditor;
using UnityEngine;
using WUI.Editor.Enumerations;
using WUI.Utilities;

namespace WUI.Runtime.ScriptableObjects
{
    [CustomEditor(typeof(WUI_UI_SO))]
    public class WUI_UI_SO_Editor : UnityEditor.Editor
    {
        private WUI_UI_SO _uiData;

        private void OnEnable() => _uiData = target as WUI_UI_SO;

        public override void OnInspectorGUI()
        {
            #region UI Title

            WUI_EditorUtilities.TitleWithIcon_Gray("Node", "UI Node");
            
            #endregion

            #region UI Name

            WUI_EditorUtilities.LabelFieldAndText("UI Name", _uiData.UIName, out var newUIName);
            _uiData.UIName = newUIName;

            #endregion

            #region If UI

            if (_uiData.NodeType is WUI_NodeType.BasicUI or WUI_NodeType.HomeUI or WUI_NodeType.LastUI)
            {
                WUI_EditorUtilities.TextField("UI Information", _uiData.UIInformation, out var newUIInformation);
                _uiData.UIInformation = newUIInformation;
            }

            #endregion

            #region If Wait Time

            if (_uiData.NodeType is WUI_NodeType.WaitTime)
            {
                WUI_EditorUtilities.FloatField("Wait Time", 0, out _);
            }

            #endregion

            if (_uiData.NodeType != WUI_NodeType.HomeUI)
            {
                WUI_EditorUtilities.TitleWithIcon_Orange("Enter", "Previous Node");
                
                #region Previous Node Name

                WUI_EditorUtilities.TextField("Previous UI Name", _uiData.PreviousUI.Text, out var newPreviousText);
                _uiData.PreviousUI.Text = newPreviousText;

                #endregion
            }

            if (_uiData.NodeType != WUI_NodeType.LastUI)
            {
                WUI_EditorUtilities.TitleWithIcon_Blue("Exit", "Next Node");
                
                #region Next Node Name

                WUI_EditorUtilities.TextField("Next UI Name", _uiData.NextUI.Text, out var newNextText);
                _uiData.NextUI.Text = newNextText;

                #endregion
            }
        }
    }
}