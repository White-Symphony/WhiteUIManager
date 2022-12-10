using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Editor.Elements;

namespace WUI.Editor.Utilities
{
    public static class WUI_ElementUtility
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            var button = new Button(onClick)
            {
                text = text
            };

            return button;
        }

        public static void CreatePort(
            this WUI_Node node,
            out Port port,
            string portName = "",
            Color portColor = default,
            Orientation orientation = Orientation.Horizontal,
            Direction direction = Direction.Input,
            Port.Capacity capacity = Port.Capacity.Single,
            Type type = default)
        {
            port = node.InstantiatePort(orientation, direction, capacity, type);

            port.portName = portName;

            port.portColor = portColor;
        }

        public static Foldout CreateFoldout(string title, bool collapsed = true)
        {
            var foldout = new Foldout
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }
        
        public static TextField CreateTextField(string value = null,
            string label = null,
            EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            var textField = new TextField
            {
                value = value,
                label = label
            };
            
            if(onValueChanged != null)
                textField.RegisterValueChangedCallback(onValueChanged);

            return textField;
        }

        public static TextField CreateTextArea(string value = null,
            string label = null,
            EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            var textArea = CreateTextField(value, label, onValueChanged);

            textArea.multiline = true;

            return textArea;
        }
    }
}