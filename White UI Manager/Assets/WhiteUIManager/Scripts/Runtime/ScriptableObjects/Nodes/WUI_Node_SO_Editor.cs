using System.Linq;
using UnityEditor;
using UnityEngine;
using WUI.Editor.Enumerations;
using WUI.Utilities;

namespace WUI.Runtime.ScriptableObjects
{
    [CustomEditor(typeof(WUI_Node_SO))]
    public class WUI_Node_SO_Editor : UnityEditor.Editor
    {
        protected WUI_Node_SO _nodeData;

        protected virtual void OnEnable() => _nodeData = target as WUI_Node_SO;

        public override void OnInspectorGUI()
        {
            #region Node Title

            WUI_EditorUtilities.TitleWithIcon_Gray("Node", "Node");
            
            #endregion

            #region Node Name

            WUI_EditorUtilities.LabelFieldAndText("Node Name", _nodeData.NodeName, out var newUIName);
            _nodeData.NodeName = newUIName;

            #endregion

            if (_nodeData.NodeType != WUI_NodeType.HomeUI)
            {
                WUI_EditorUtilities.TitleWithIcon_Orange("Enter", "Previous Node");
                
                #region Previous Nodes Name

                WUI_EditorUtilities.TextsField("Previous Node Name", _nodeData.PreviousNodes.Select(n => n.NodeName).ToArray());

                #endregion
            }

            if (_nodeData.NodeType != WUI_NodeType.LastUI)
            {
                WUI_EditorUtilities.TitleWithIcon_Blue("Exit", "Next Node");
                
                #region Next Node Name

                WUI_EditorUtilities.TextsField("Next Node Name", _nodeData.NextNodes.Select(n => n.NodeName).ToArray());

                #endregion
            }
        }
    }
}