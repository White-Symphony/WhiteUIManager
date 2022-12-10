using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace WUI.Editor.Elements
{
    using Enumerations;
    using Graph;
    using Data.Save;
    
    public class WUI_LastUI_Node: WUI_Node
    {
        public override void Draw()
        {
            UIType = WUI_UIType.LastUI;
            
            base.Draw();

            AddUIInfo();
            AddOpenButton();
        }

        public override void Initialize(string nodeName, WUI_GraphView graphView, WUI_UIType uiType, Vector2 position)
        {
            base.Initialize(nodeName, graphView, uiType, position);

            PreviousUI = new WUI_UISaveData();

            AddInput("Previous UI", PreviousUI, Port.Capacity.Multi);
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