using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace WUI.Editor.Manipulator
{
    public class WUI_GraphView_Manipulator : WUI_IManipulator
    {
        private readonly List<IManipulator> _manipulators;

        public WUI_GraphView_Manipulator()
        {
            _manipulators = new List<IManipulator>
            {
                new ContentZoomer(),
                new ContentDragger(),
                new SelectionDragger(),
                new RectangleSelector(),
            };
        }
        
        public override void SetManipulator(VisualElement element)
        {
            var cloneManipulators = _manipulators.ToList();
            
            cloneManipulators.ForEach(m =>
            {
                element.AddManipulator(m);

                _manipulators.Remove(m);
            });
        }

        public override void AddManipulator(VisualElement element, IManipulator manipulator)
        {
            _manipulators.Add(manipulator);
            
            SetManipulator(element);
        }
    }
}