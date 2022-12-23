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
        public override void Draw()
        {
            NodeType = WUI_NodeType.WaitTime;

            AddFloatField();

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

            NextUI = new WUI_UISaveData();

            PreviousUI = new WUI_UISaveData();

            AddInput("", PreviousUI, Port.Capacity.Multi);
            AddOutput("", NextUI, Port.Capacity.Multi);
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

        private void AddFloatField()
        {
            var textField = new FloatField(5);

            outputContainer.Add(textField);
        }

        #endregion
    }
}