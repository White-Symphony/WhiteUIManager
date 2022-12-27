using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace WUI.Editor.Elements
{
    public class WUI_EdgeConnector : EdgeConnector
    {
        protected override void RegisterCallbacksOnTarget()
        {
            throw new System.NotImplementedException();
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            throw new System.NotImplementedException();
        }

        public override EdgeDragHelper edgeDragHelper { get; }
    }
}