using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Editor.Data.Save;
using WUI.Editor.Graph;
using WUI.Utilities;

namespace WUI.Editor.Elements
{
    using Enumerations;
    using Utilities;
    
    public class WUI_Node : Node
    {
        public string ID { get; set; }

        public string UIName { get; protected set; }

        public string UIInformation { get; protected set; }

        public WUI_UISaveData PreviousUI { get; set; }
        
        public WUI_UISaveData NextUI { get; set; }
        
        public WUI_UIType UIType { get; protected set; }
        
        public WUI_Group Group { get; set; }

        protected WUI_GraphView _graphView;
        
        private Color _defaultBackgroundColor;

        public virtual void Initialize(string nodeName, WUI_GraphView graphView, WUI_UIType uiType, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            
            _graphView = graphView;

            PreviousUI = new WUI_UISaveData();
            NextUI = new WUI_UISaveData();
            
            UIType = uiType;
            UIName = nodeName;

            _defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f, .3f);
            
            ResetStyle();
            
            SetPosition(new Rect(position, Vector2.one));

            mainContainer.AddClasses("wui-node_main-container");
            extensionContainer.AddClasses("wui-node__extension-container");
        }

        public bool IsStartingNode()
        {
            var inputPort = inputContainer.childCount;

            return inputPort < 1;
        }
        
        public virtual void Draw()
        {
            #region TITLE

            var titleTextField = WUI_ElementUtility.CreateTextField(UIName, null, callback =>
            {
                if (callback.target is not TextField textField) return;

                textField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (userData is WUI_UISaveData saveData)
                    saveData.Text = textField.value;

                if (string.IsNullOrEmpty(textField.value))
                {
                    if (!string.IsNullOrEmpty(UIName))
                    {
                        ++_graphView.NamesErrorAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(UIName))
                    {
                        --_graphView.NamesErrorAmount;
                    }
                }
                
                if (Group == null)
                {
                    _graphView.RemoveUngroupedNode(this);

                    UIName = textField.value;
                    
                    _graphView.AddUngroupedNode(this);

                    return;
                }

                var currentGroup = Group;
                
                _graphView.RemoveGroupedNode(this, Group);
                
                UIName = textField.value;

                _graphView.AddGroupedNode(this, currentGroup);
            });

            titleTextField.AddClasses(
                "wui-node__textfield",
                "wui-node__filename-textfield",
                "wui-node__textfield_hidden");

            titleContainer.Insert(0, titleTextField);

            #endregion

            RefreshExpandedState();
        }

        protected void AddOpenButton()
        {
            var openButton = WUI_ElementUtility.CreateButton("Open This UI");

            openButton.AddClasses(
                "wui-node__button",
                "wui-node__button:hover",
                "wui-node__button:focus");

            mainContainer.Insert(0, openButton);
            
            RefreshExpandedState();
        }
        
        protected void AddUIInfo()
        {
            var customDataContainer = new VisualElement();

            customDataContainer.AddClasses("wui-node__custom-data-container");

            var infoFoldout = WUI_ElementUtility.CreateFoldout("UI Information");

            var infoTextField = WUI_ElementUtility.CreateTextArea(UIInformation, onValueChanged: e =>
            {
                UIInformation = e.newValue;
            });

            infoTextField.AddClasses(
                "wui-node__textfield",
                "wui-node__quote-textfield");

            infoFoldout.Add(infoTextField);
            
            customDataContainer.Add(infoFoldout);
            
            extensionContainer.Add(customDataContainer);
            
            RefreshExpandedState();
        }
        
        public void AddInput(string inputName, object ui_userData, Port.Capacity capacity = Port.Capacity.Single)
        {
            this.CreatePort(
                out var port,
                inputName,
                new Color(1f, 0.61f, 0.23f),
                Orientation.Horizontal,
                Direction.Input,
                capacity,
                typeof(string));

            port.userData = ui_userData;

            inputContainer.Add(port);

            RefreshExpandedState();
        }
        
        public void AddOutput(string inputName, object ui_userData, Port.Capacity capacity = Port.Capacity.Single)
        {
            this.CreatePort(
                out var port,
                inputName,
                new Color(0.58f, 1f, 0.25f),
                Orientation.Horizontal,
                Direction.Output,
                capacity,
                typeof(string));

            port.userData = ui_userData;

            outputContainer.Add(port);

            RefreshExpandedState();
        }

        #region Utility Methods

        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }

        protected void DisconnectInputPorts() => DisconnectPorts(inputContainer);

        protected void DisconnectOutputPorts() => DisconnectPorts(outputContainer);
        
        private void DisconnectPorts(VisualElement container)
        {
            foreach (var element in container.Children())
            {
                if (element is not Port port) return;

                if (!port.connected) continue;

                var edges = new List<Edge>(port.connections);
                
                _graphView.DeleteElements(edges);
            }
        }
        
        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = _defaultBackgroundColor;
        }
        
        #endregion
    }
}