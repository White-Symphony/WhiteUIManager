using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Editor.Graph.Feature;

namespace WUI.Editor.Graph
{
    public class WUI_Graph_MiniMap : WUI_Graph_Feature
    {
        private MiniMap _miniMap;

        public MiniMap GetMap() => _miniMap;
        
        public void Initialize(bool isAnchored, Rect rect = default, bool isVisible = false)
        {
            _miniMap = new MiniMap { anchored = isAnchored };
            
            _miniMap.SetPosition(rect);

            ChangeVisibleState(isVisible);
        }

        public void SetPosition(Rect rect) => _miniMap.SetPosition(rect);

        public override void ChangeVisibleState(bool state) => _miniMap.visible = state;

        public override bool GetVisibleState() => _miniMap.visible;

        public override void Add(VisualElement element) { }

        public override void AddToElement(VisualElement element) => element.Add(_miniMap);
        
        public override void Insert(VisualElement element, int index) { }

        public override void SetStyle()
        {
            var backgroundColor = new StyleColor(new Color32(4, 15, 22, 255));
            var borderColor = new StyleColor(new Color32(94, 116, 127, 255));
            
            _miniMap.style.backgroundColor = backgroundColor;
            _miniMap.style.borderBottomColor = borderColor;
            _miniMap.style.borderTopColor = borderColor;
            _miniMap.style.borderLeftColor = borderColor;
            _miniMap.style.borderRightColor = borderColor;
        }
    }
}