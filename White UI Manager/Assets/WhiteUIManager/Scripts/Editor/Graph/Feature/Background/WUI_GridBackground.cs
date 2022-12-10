using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace WUI.Editor.Graph.Feature
{
    public abstract class WUI_GridBackground : WUI_Graph_Feature
    {
        protected readonly GridBackground _gridBackground;

        protected WUI_GridBackground()
        {
            _gridBackground = new GridBackground();
        }

        public virtual GridBackground GetBackground() => _gridBackground;
        
        public virtual void StretchToParentSize() => _gridBackground.StretchToParentSize();
        
        public virtual void StretchToParentWidth() => _gridBackground.StretchToParentWidth();
        
        public override void Add(VisualElement element) { }

        public override void AddToElement(VisualElement element) { }

        public override void Insert(VisualElement element, int index) { }

        public override void SetStyle() { }

        public override void ChangeVisibleState(bool state) => _gridBackground.visible = state;

        public override bool GetVisibleState() => _gridBackground.visible;
    }
}