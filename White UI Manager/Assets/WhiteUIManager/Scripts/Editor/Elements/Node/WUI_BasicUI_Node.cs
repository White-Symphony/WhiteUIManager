using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Utilities;

namespace WUI.Editor.Elements
{
    using Enumerations;
    using Graph;
    using Data.Save;
    
    public class WUI_BasicUI_Node : WUI_Node
    {
        public override void Draw()
        {
            NodeType = WUI_NodeType.BasicUI;
            
            base.Draw();
            
            AddOpenButton();
        }

        protected override void SetIcon()
        {
            _icon = new Image
            {
                image = WUI_EditorUtilities.GetBlackIcon("Node")
            };
            
            base.SetIcon();
        }

        #region Override Methods
        
        public override void Initialize(string nodeName, WUI_GraphView graphView, WUI_NodeType nodeType, Vector2 position)
        {
            base.Initialize(nodeName, graphView, nodeType, position);

            NextNodes = new List<WUI_NodeData> { new() };
            PreviousNodes = new List<WUI_NodeData> { new() };
            
            foreach (var previousNode in PreviousNodes)
            {
                AddInput("", previousNode);
            }
            
            foreach (var nextNode in NextNodes)
            {
                AddOutput("", nextNode);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            evt.menu.AppendAction("Disconnect Previous Ports", _ => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Next Ports", _ => DisconnectOutputPorts());
        }

        #endregion
    }
}