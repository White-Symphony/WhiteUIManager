using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WUI.Editor.Enumerations;

namespace WUI.Runtime.ScriptableObjects
{
    [CustomEditor(typeof(WUI_UI_SO))]
    public class WUI_UI_SO_Editor : UnityEditor.Editor
    {
        private WUI_UI_SO _uiData;

        private GUIStyle _boxStyle;
        
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        
        private void OnEnable()
        {
            _uiData = target as WUI_UI_SO;

            _boxStyle = new GUIStyle
            {
                normal =
                {
                    
                }
            };
            
            _titleStyle = new GUIStyle
            {
                fontSize = 25,
                normal =
                {
                    textColor = new Color(1f, 0.79f, 0.23f)
                },
                fixedWidth = 50
            };
            
            _labelStyle = new GUIStyle
            {
                fontSize = 12,
                normal =
                {
                    textColor = Color.white
                }
            };
        }

        public override void OnInspectorGUI()
        {
            #region UI Title

            Begin_H(_boxStyle);

            var texture = new Texture2D(32, 32);
            
            texture = SetColor(texture, Color.white);

            GUILayout.Box(texture);

            #region UI Node

            Begin_V(EditorStyles.helpBox);

            EditorGUILayout.LabelField("UI Node", _titleStyle);
            
            EditorGUILayout.Space(10);
            
            End_V();

            #endregion
            
            End_H();
            
            #endregion

            #region UI Name

            Begin_V(EditorStyles.helpBox);

            EditorGUILayout.LabelField("UI Name", _labelStyle);
            _uiData.UIName = EditorGUILayout.TextField(_uiData.UIName);
            
            End_V();

            #endregion

            #region UI Information

            Begin_V(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("UI Information", _labelStyle);
            _uiData.UIInformation = EditorGUILayout.TextArea(_uiData.UIInformation);
            
            End_V();

            #endregion

            if (_uiData.UIType != WUI_UIType.FirstUI)
            {
                #region Previous Node

                Begin_V(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Previous Node", _titleStyle);
            
                EditorGUILayout.Space(10);
            
                End_V();
            
                #region Previous UI Name

                Begin_V(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Previous UI Name", _labelStyle);
                _uiData.PreviousUI.Text = EditorGUILayout.TextField(_uiData.PreviousUI.Text);
            
                End_V();

                #endregion

                #endregion
            }

            if (_uiData.UIType != WUI_UIType.LastUI)
            {
                #region Next Node

                Begin_V(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Next Node", _titleStyle);
            
                EditorGUILayout.Space(10);
            
                End_V();

                #region Next UI Name

                Begin_V(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Next UI Name", _labelStyle);
                _uiData.NextUI.Text = EditorGUILayout.TextField(_uiData.NextUI.Text);
            
                End_V();

                #endregion
            
                #endregion
            }
        }

        #region Utilities

        private static Texture2D SetColor(Texture2D texture, Color color)
        {
            var pixels = texture.GetPixels();
            
            for (var i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }
            
            texture.SetPixels(pixels);
            
            texture.Apply();

            return texture;
        }
        
        private void Begin_V(GUIStyle guiStyle) => EditorGUILayout.BeginVertical(guiStyle);

        private void End_V() => EditorGUILayout.EndVertical();
        
        private void Begin_H(GUIStyle guiStyle) => EditorGUILayout.BeginHorizontal(guiStyle);

        private void End_H() => EditorGUILayout.EndHorizontal();

        #endregion
    }
}