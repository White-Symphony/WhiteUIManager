using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WUI.Global.Database;

namespace WUI.Utilities
{
    public static class WUI_EditorUtilities
    {
        #region Style Variables

        public static readonly WUI_EditorStyles Styles = new ();

        #endregion

        #region Title

        public static void TitleWithIcon_Gray(string iconName, string title)
        {
            Begin_H(Styles.BoxStyle);

            GUILayout.Box(GetColouredIcon(iconName));

            LabelField_Gray(title);

            End_H();
        }
        
        public static void TitleWithIcon_Orange(string iconName, string title)
        {
            Begin_H(Styles.BoxStyle);

            GUILayout.Box(GetColouredIcon(iconName));

            LabelField_Orange(title);

            End_H();
        }
        
        public static void TitleWithIcon_Blue(string iconName, string title)
        {
            Begin_H(Styles.BoxStyle);

            GUILayout.Box(GetColouredIcon(iconName));

            LabelField_Blue(title);

            End_H();
        }
        
        public static void TitleWithIcon_Red(string iconName, string title)
        {
            Begin_H(Styles.BoxStyle);

            GUILayout.Box(GetColouredIcon(iconName));

            LabelField_Red(title);

            End_H();
        }

        #endregion

        #region Field Methods

        public static void TextsField(string field, params string[] inTexts)
        {
            Begin_V(Styles.HelpBox);
            
            EditorGUILayout.LabelField(field, Styles.LabelStyle);
            
            EditorGUILayout.Space(10);

            foreach (var inText in inTexts)
            {
                EditorGUILayout.TextField(inText, Styles.Field);   
                EditorGUILayout.Space(10);
            }

            EditorGUILayout.Space(10);

            End_V();
        }
        
        public static void TextField(string field, string inText, out string outText)
        {
            outText = inText;
            
            Begin_V(Styles.HelpBox);
            
            EditorGUILayout.LabelField(field, Styles.LabelStyle);
            
            EditorGUILayout.Space(10);
            
            outText = EditorGUILayout.TextField(outText, Styles.Field);
            
            EditorGUILayout.Space(10);

            End_V();
        }
        
        public static void FloatField(string field, float inValue, out float outValue)
        {
            outValue = inValue;
            
            Begin_V(Styles.HelpBox);
            
            EditorGUILayout.LabelField(field, Styles.LabelStyle);
            
            EditorGUILayout.Space(10);
            
            outValue = EditorGUILayout.FloatField(outValue, Styles.Field);
            
            EditorGUILayout.Space(10);

            End_V();
        }

        #endregion
        
        #region Label Methods
        
        public static void LabelField_Gray(string field)
        {
            Begin_V(Styles.HelpBox);

            EditorGUILayout.LabelField(field, Styles.TitleStyle_Gray);
            
            EditorGUILayout.Space(10);
            
            End_V();
        }

        public static void LabelField_Orange(string field)
        {
            Begin_V(Styles.HelpBox);

            EditorGUILayout.LabelField(field, Styles.TitleStyle_Orange);
            
            EditorGUILayout.Space(10);
            
            End_V();
        }
        
        public static void LabelField_Blue(string field)
        {
            Begin_V(Styles.HelpBox);

            EditorGUILayout.LabelField(field, Styles.TitleStyle_Blue);
            
            EditorGUILayout.Space(10);
            
            End_V();
        }
        
        public static void LabelField_Red(string field)
        {
            Begin_V(Styles.HelpBox);

            EditorGUILayout.LabelField(field, Styles.TitleStyle_Red);
            
            EditorGUILayout.Space(10);
            
            End_V();
        }

        public static void LabelFieldAndText(string field, string inText, out string outText)
        {
            outText = inText;
            
            Begin_V(Styles.HelpBox);

            EditorGUILayout.LabelField(field, Styles.TitleStyle_Orange);
            
            EditorGUILayout.Space(10);
            
            outText = EditorGUILayout.TextArea(outText, Styles.Field);
            
            EditorGUILayout.Space(10);

            End_V();
        }
        
        public static void LabelFieldAndFloat(string field, float inValue, out float outValue)
        {
            outValue = inValue;
            
            Begin_V(Styles.HelpBox);

            EditorGUILayout.LabelField(field, Styles.TitleStyle_Orange);
            
            EditorGUILayout.Space(10);
            
            outValue =EditorGUILayout.FloatField(outValue, Styles.TextStyle);
            
            EditorGUILayout.Space(10);
            
            End_V();
        }

        #endregion

        #region TextArea Methods

        public static void TextArea(string field, string inText, out string outText)
        {
            outText = inText;
            
            Begin_V(Styles.HelpBox);
            
            EditorGUILayout.LabelField(field, Styles.LabelStyle);
            
            EditorGUILayout.Space(10);
            
            outText = EditorGUILayout.TextArea(outText, Styles.TextStyle);
            
            EditorGUILayout.Space(10);

            End_V();
        }

        #endregion
        
        #region Utilities

        public static Texture2D GetBlackIcon(string iconName) => WUI_WhiteDatabase.TextureDatabase.GetTexture("Black", iconName);
        
        public static Texture2D GetColouredIcon(string iconName) => WUI_WhiteDatabase.TextureDatabase.GetTexture("Coloured", iconName);
        
        public static Texture2D SetColor(Texture2D texture, Color color)
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

        public static void Begin_V(GUIStyle guiStyle) => EditorGUILayout.BeginVertical(guiStyle);

        public static void End_V() => EditorGUILayout.EndVertical();
        
        public static void Begin_H(GUIStyle guiStyle) => EditorGUILayout.BeginHorizontal(guiStyle);

        public static void End_H() => EditorGUILayout.EndHorizontal();
        
        #endregion
    }

    public class WUI_EditorStyles
    {
        private static readonly GUISkin guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/WhiteUIManager/Skins/WhiteGUISkin.guiskin");
        
        #region Styles

        public readonly GUIStyle Field = guiSkin.textField;
        
        public readonly GUIStyle HelpBox = guiSkin.box;
        
        public readonly GUIStyle BoxStyle = new ()
        {
            normal =
            {
                background = null,
                textColor = default,
                scaledBackgrounds = new Texture2D[] { }
            }
        };
          
        public readonly GUIStyle TitleStyle_Gray = new ()
        {
            fontSize = 25,
            normal =
            {
                textColor = new Color(0.37f, 0.45f, 0.5f)
            },
            fixedWidth = 50
        };
        
        public readonly GUIStyle TitleStyle_Orange = new ()
        {
            fontSize = 25,
            normal =
            {
                textColor = new Color(1f, 0.79f, 0.23f)
            },
            fixedWidth = 50
        };
        
        public readonly GUIStyle TitleStyle_Blue = new ()
        {
            fontSize = 25,
            normal =
            {
                textColor = new Color(0.33f, 0.87f, 0.99f)
            },
            fixedWidth = 50
        };
        
        public readonly GUIStyle TitleStyle_Red = new ()
        {
            fontSize = 25,
            normal =
            {
                textColor = new Color(1f, 0.35f, 0.37f)
            },
            fixedWidth = 50
        };
            
        public readonly GUIStyle LabelStyle = new()
        {
            fontSize = 12,
            normal =
            {
                textColor = Color.white
            }
        };

        public readonly GUIStyle TextStyle = new ()
        {
            fontSize = 20,
            normal =
            {
                textColor = Color.white,
                background = WUI_EditorUtilities.SetColor(new Texture2D(1,1), new Color(0.16f, 0.16f, 0.16f))
            }
        };

        #endregion
    }
}