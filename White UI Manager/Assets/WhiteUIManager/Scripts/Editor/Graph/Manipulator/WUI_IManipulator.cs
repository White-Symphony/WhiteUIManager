using UnityEngine;
using UnityEngine.UIElements;

namespace WUI.Editor.Manipulator
{
    public abstract class WUI_IManipulator
    {
        public abstract void SetManipulator(VisualElement element);
        
        public abstract void AddManipulator(VisualElement element, IManipulator manipulator);
    }
}