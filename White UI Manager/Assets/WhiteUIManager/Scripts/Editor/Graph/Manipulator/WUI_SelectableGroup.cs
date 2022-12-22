using UnityEditor;
using UnityEngine.UIElements;
using WUI.Editor.Elements;
using WUI.Editor.Utilities;

namespace WUI.Editor.Manipulator
{
    public class WUI_SelectableGroup : UnityEngine.UIElements.Manipulator
    {
        private readonly WUI_Group _group;
        
        public WUI_SelectableGroup(WUI_Group group)
        {
            _group = group;
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
            var groupData = WUI_IOUtility.GetGroupByID(_group.ID);

            Selection.activeObject = groupData;
        }
    }
}