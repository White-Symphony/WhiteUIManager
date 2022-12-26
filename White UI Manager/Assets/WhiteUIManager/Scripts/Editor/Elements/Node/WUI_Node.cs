using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Editor.Data.Save;
using WUI.Editor.Graph;
using WUI.Editor.Manipulator;
using WUI.Runtime.ScriptableObjects;
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

        public List<WUI_NodeData> PreviousNodes { get; set; }
        
        public List<WUI_NodeData> NextNodes { get; set; }
        
        public WUI_NodeType NodeType { get; protected set; }
        
        public WUI_Group Group { get; set; }

        protected WUI_GraphView _graphView;

        protected Image _icon;
        
        private Color _defaultBackgroundColor;

        public virtual void Initialize(WUI_Node_SO nodeData, WUI_GraphView graphView, WUI_NodeType nodeType)
        {
            AddManipulators();
            
            ID = Guid.NewGuid().ToString();
            
            _graphView = graphView;

            NodeType = nodeType;
            UIName = nodeData.NodeName;

            NextNodes = nodeData.NextNodes?.ToList();
            PreviousNodes = nodeData.PreviousNodes?.ToList();

            _defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f, .3f);
            
            ResetStyle();
            
            SetPosition(new Rect(nodeData.Position, Vector2.one));

            mainContainer.AddClasses("wui-node_main-container");
            extensionContainer.AddClasses("wui-node__extension-container");
            
            if(PreviousNodes != null) foreach (var previousNode in PreviousNodes) AddInput("", previousNode);
            if(NextNodes != null) foreach (var nextNode in NextNodes) AddOutput("", nextNode);   
        }
        
        public virtual void Initialize(string nodeName, WUI_GraphView graphView, WUI_NodeType nodeType, Vector2 position)
        {
            AddManipulators();
            
            ID = Guid.NewGuid().ToString();
            
            _graphView = graphView;

            NodeType = nodeType;
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
            #region Title

            SetIcon();
            
            var titleTextField = WUI_ElementUtility.CreateTextField(UIName, null, callback =>
            {
                if (callback.target is not TextField textField) return;

                textField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (userData is WUI_NodeData saveData)
                    saveData.NodeName = textField.value;

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
                
                var nodeData = WUI_IOUtility.GetNodeByID(ID);

                if (Group == null)
                {
                    _graphView.RemoveUngroupedNode(this);

                    UIName = textField.value;

                    nodeData.NodeName = UIName;

                    WUI_IOUtility.DirtyAsset(nodeData);
                    
                    WUI_IOUtility.UpdateNodeConnection(nodeData);

                    _graphView.AddUngroupedNode(this);

                    return;
                }

                var currentGroup = Group;
                
                _graphView.RemoveGroupedNode(this, Group);

                UIName = textField.value;

                nodeData.NodeName = UIName;

                WUI_IOUtility.DirtyAsset(nodeData);
                
                WUI_IOUtility.UpdateNodeConnection(nodeData);

                _graphView.AddGroupedNode(this, currentGroup);
            });

            titleTextField.AddClasses(
                "wui-node__textfield",
                "wui-node__filename-textfield",
                "wui-node__textfield_hidden");
            
            titleContainer.Insert(1, titleTextField);

            #endregion

            #region Input Container

            inputContainer.style.borderBottomWidth = 5;
            inputContainer.style.borderTopWidth = 5;
            inputContainer.style.borderRightWidth = 5;
            inputContainer.style.borderLeftWidth = 5;
            
            outputContainer.style.borderBottomWidth = 5;
            outputContainer.style.borderTopWidth = 5;
            outputContainer.style.borderRightWidth = 5;
            outputContainer.style.borderLeftWidth = 5;

            #endregion

            RefreshExpandedState();
        }

        #region Extensions

        protected virtual void SetIcon()
        {
            titleContainer.Insert(0, _icon);
            
            RefreshExpandedState();
        }
        
        protected virtual void AddOpenButton()
        {
            var openButton = WUI_ElementUtility.CreateButton("Open This UI");

            openButton.AddClasses(
                "wui-node__button",
                "wui-node__button:hover",
                "wui-node__button:focus");

            mainContainer.Insert(0, openButton);
            
            RefreshExpandedState();
        }
        
        protected virtual void AddUIInfo()
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

        #endregion

        #region Ports

        public new WUI_Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return WUI_Port.Create<WUI_Edge>(orientation, direction, capacity, type);
        }

        public void AddInput(string inputName, object ui_userData, Port.Capacity capacity = Port.Capacity.Single)
        {
            this.CreatePort(
                out var port,
                inputName,
                new Color(1f, 0.79f, 0.23f),
                Orientation.Horizontal,
                Direction.Input,
                capacity,
                typeof(string));

            port.userData = ui_userData;

            inputContainer.Add(port);

            RefreshExpandedState();
        }

        public void AddInputWithData(string inputName, object ui_userData,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            if (PreviousNodes == null) return;
            
            PreviousNodes.Add(new WUI_NodeData());
            AddInput(inputName, ui_userData, capacity);
        }

        public void AddOutput(string inputName, object ui_userData, Port.Capacity capacity = Port.Capacity.Single)
        {
            this.CreatePort(
                out var port,
                inputName,
                new Color(0.33f, 0.87f, 0.99f),
                Orientation.Horizontal,
                Direction.Output,
                capacity,
                typeof(string));

            port.userData = ui_userData;

            outputContainer.Add(port);

            RefreshExpandedState();
        }

        public void AddOutputWithData(string inputName, object ui_userData,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            if (NextNodes == null) return;
            
            NextNodes.Add(new WUI_NodeData());
            AddOutput(inputName, ui_userData, capacity);
        }

        public bool RemoveLastInput()
        {
            if (inputContainer.Children().Last() is not Port port) return false;

            if (port.connected) return false;

            PreviousNodes.Remove(PreviousNodes[^1]);
            inputContainer.Remove(port);
            
            RefreshExpandedState();

            return true;
        }

        public bool RemoveLastOutput()
        {
            if (outputContainer.Children().Last() is not Port port) return false;
            
            if (port.connected) return false;

            NextNodes.Remove(NextNodes[^1]);
            outputContainer.Remove(port);
            
            RefreshExpandedState();

            return true;
        }

        #endregion

        #region Utility Methods

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = _defaultBackgroundColor;
        }
        
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

        private void AddManipulators()
        {
            contentContainer.AddManipulator(new WUI_SelectableNode(this));
        }
        
        #endregion
    }
}