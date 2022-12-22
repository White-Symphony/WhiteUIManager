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
    
    public class WUI_HomeUI_Node: WUI_Node
    {
        public override void Draw()
        {
            NodeType = WUI_NodeType.HomeUI;
            
            base.Draw();

            AddUIInfo();
            AddOpenButton();
        }

        protected override void SetIcon()
        {
            _icon = new Image
            {
                image = WUI_EditorUtilities.GetBlackIcon("Home")
            };
            
            base.SetIcon();
        }
        
        public override void Initialize(string nodeName, WUI_GraphView graphView, WUI_NodeType nodeType, Vector2 position)
        {
            base.Initialize(nodeName, graphView, nodeType, position);

            NextUI = new WUI_UISaveData();

            AddOutput("", NextUI, Port.Capacity.Multi);
        }

        #region Override Methods

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            
            evt.menu.AppendAction("Disconnect Next Ports", _ => DisconnectOutputPorts());
        }

        #endregion
    }
}