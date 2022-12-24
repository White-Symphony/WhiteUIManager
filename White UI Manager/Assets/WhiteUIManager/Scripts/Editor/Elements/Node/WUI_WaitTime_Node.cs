using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Utilities;

namespace WUI.Editor.Elements
{
    using Graph;
    using Enumerations;
    using Data.Save;
    
    public class WUI_WaitTime_Node : WUI_Node
    {
        public float WaitTime;

        private Label timeLabel;
        
        public override void Draw()
        {
            NodeType = WUI_NodeType.WaitTime;

            AddFloatField();

            AddTimeLabel();

            base.Draw();
        }

        protected override void SetIcon()
        {
            _icon = new Image
            {
                image = WUI_EditorUtilities.GetBlackIcon("Clock")
            };
            
            base.SetIcon();
        }

        public override void Initialize(string nodeName, WUI_GraphView graphView, WUI_NodeType nodeType, Vector2 position)
        {
            base.Initialize(nodeName, graphView, nodeType, position);

            NextNodes = new List<WUI_NodeData>{new ()};

            PreviousNodes = new List<WUI_NodeData>{new ()};

            foreach (var previousNode in PreviousNodes)
            {
                AddInput("", previousNode);   
            }

            foreach (var nextNode in NextNodes)
            {
                AddOutput("", nextNode);
            }
        }

        #region Override Methods

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            evt.menu.AppendAction("Disconnect Previous Ports", _ => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Next Ports", _ => DisconnectOutputPorts());
        }

        #endregion

        #region Extension

        private void AddTimeLabel()
        {
            timeLabel = new Label($"Wait {WaitTime}\nSecond")
            {
                style =
                {
                    marginTop = 5,
                    position = Position.Absolute
                }
            };

            outputContainer.Add(timeLabel);
        }
        
        private void AddFloatField()
        {
            var textField = new FloatField(2)
            {
                style =
                {
                    paddingTop = 6,
                    paddingLeft = 6,
                    width = 32,
                    position = Position.Absolute,
        
                    left = 15
                }
            };

            textField.RegisterValueChangedCallback(@event =>
            {
                WaitTime = @event.newValue;
                timeLabel.text = $"Wait {WaitTime}\nSecond";
            });

            inputContainer.Add(textField);
        }

        #endregion
    }
}