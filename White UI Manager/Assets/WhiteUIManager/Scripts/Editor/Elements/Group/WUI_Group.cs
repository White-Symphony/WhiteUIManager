using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using WUI.Editor.Utilities;

namespace WUI.Editor.Elements
{
    public sealed class WUI_Group : Group
    {
        public string ID { get; set; }
        
        public string OldTitle { get; set; }
        
        private readonly Color _defaultBorderColor;
        private readonly float _defaultBorderWidth;

        public WUI_Group(string groupTitle, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            
            title = groupTitle;

            OldTitle = groupTitle;
            
            SetPosition(new Rect(position, Vector2.zero));
            
            _defaultBorderColor = contentContainer.style.borderTopColor.value;
            _defaultBorderWidth = contentContainer.style.borderTopWidth.value;

            headerContainer.AddClasses("wui-group__header-container");
        }

        public void SetErrorStyle(Color color)
        {
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderBottomWidth = 2f;
        }

        public void ResetStyle()
        {
            contentContainer.style.borderBottomColor = _defaultBorderColor;
            contentContainer.style.borderBottomWidth = _defaultBorderWidth;
        }
    }
}