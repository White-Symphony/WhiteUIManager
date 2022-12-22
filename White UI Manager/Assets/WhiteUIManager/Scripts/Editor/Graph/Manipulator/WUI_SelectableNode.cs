using UnityEditor;
using UnityEngine.UIElements;
using WUI.Editor.Elements;
using WUI.Editor.Utilities;

namespace WUI.Editor.Manipulator
{
    public class WUI_SelectableNode : UnityEngine.UIElements.Manipulator
    {
        private readonly WUI_Node _node;
        
        public WUI_SelectableNode(WUI_Node node)
        {
            _node = node;
        }
        
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent @event)
        {
            var nodeData = WUI_IOUtility.GetNodeByID(_node.ID);

            Selection.activeObject = nodeData;
        }
    }
}