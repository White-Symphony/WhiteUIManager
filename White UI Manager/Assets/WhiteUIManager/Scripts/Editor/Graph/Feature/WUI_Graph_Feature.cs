using UnityEngine.UIElements;

namespace WUI.Editor.Graph.Feature
{
    public abstract class WUI_Graph_Feature
    {
        public abstract void Add(VisualElement element);

        public abstract void AddToElement(VisualElement element);

        public abstract void Insert(VisualElement element, int index);

        public abstract void SetStyle();

        public abstract void ChangeVisibleState(bool state);
        
        public abstract bool GetVisibleState();
    }
}