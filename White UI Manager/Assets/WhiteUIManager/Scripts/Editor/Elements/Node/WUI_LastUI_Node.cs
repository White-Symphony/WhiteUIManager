using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Utilities;

namespace WUI.Editor.Elements
{
    using Enumerations;
    using Graph;
    using Data.Save;
    
    public class WUI_LastUI_Node: WUI_Node
    {
        public override void Draw()
        {
            NodeType = WUI_NodeType.LastUI;
            
            base.Draw();

            AddUIInfo();
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
        
        public override void Initialize(string nodeName, WUI_GraphView graphView, WUI_NodeType nodeType, Vector2 position)
        {
            base.Initialize(nodeName, graphView, nodeType, position);

            PreviousUI = new WUI_UISaveData();

            AddInput("", PreviousUI, Port.Capacity.Multi);
        }

        #region Override Methods

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            evt.menu.AppendAction("Disconnect Previous Ports", _ => DisconnectInputPorts());
        }

        #endregion
    }
}