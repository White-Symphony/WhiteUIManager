using UnityEditor;
using UnityEngine;
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
        private GUIStyle _textStyle;

        private Texture2D _nodeTexture;
        private Texture2D _enterTexture;
        private Texture2D _exitTexture;
        
        private void OnEnable()
        {
            _uiData = target as WUI_UI_SO;

            _nodeTexture  = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WhiteUIManager/ART/Textures/Icons/Black_Node_Icon.png");
            _enterTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WhiteUIManager/ART/Textures/Icons/Black_Enter_Icon.png");
            _exitTexture  = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WhiteUIManager/ART/Textures/Icons/Black_Exit_Icon.png");
            
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

            _textStyle = new GUIStyle
            {
                fontSize = 20,
                normal =
                {
                    textColor = Color.white,
                    background = SetColor(new Texture2D(1,1), new Color(0.16f, 0.16f, 0.16f))
                }
            };
        }

        public override void OnInspectorGUI()
        {
            #region UI Title

            Begin_H(_boxStyle);

            #region Icon

            GUILayout.Box(_nodeTexture);

            #endregion

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
            _uiData.UIName = EditorGUILayout.TextArea(_uiData.UIName, _textStyle);
            
            End_V();

            #endregion

            #region UI Information

            Begin_V(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("UI Information", _labelStyle);
            _uiData.UIInformation = EditorGUILayout.TextArea(_uiData.UIInformation, _textStyle);
            
            
            End_V();

            #endregion

            if (_uiData.NodeType != WUI_NodeType.HomeUI)
            {
                #region Previous Node Title
                
                Begin_H(_boxStyle);
                
                #region Icon

                GUILayout.Box(_enterTexture);

                #endregion

                #region Text

                Begin_V(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Previous Node", _titleStyle);
            
                EditorGUILayout.Space(10);
            
                End_V();

                #endregion
                
                End_H();

                #endregion
                
                #region Previous Node Name

                Begin_V(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Previous UI Name", _labelStyle);
                _uiData.PreviousUI.Text = EditorGUILayout.TextArea(_uiData.PreviousUI.Text, _textStyle);
            
                End_V();

                #endregion
            }

            if (_uiData.NodeType != WUI_NodeType.LastUI)
            {
                #region Next Node Title
                
                Begin_H(_boxStyle);
                
                #region Icon

                GUILayout.Box(_exitTexture);

                #endregion

                #region Text

                Begin_V(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Next Node", _titleStyle);
            
                EditorGUILayout.Space(10);
            
                End_V();

                #endregion
                
                End_H();

                #endregion
                
                #region Next Node Name

                Begin_V(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Next UI Name", _labelStyle);
                _uiData.NextUI.Text = EditorGUILayout.TextArea(_uiData.NextUI.Text, _textStyle);
            
                End_V();

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