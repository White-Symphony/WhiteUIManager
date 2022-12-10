using UnityEngine;

namespace WUI.Editor.Data.Error
{
    public class WUI_ErrorData
    {
        public Color Color { get; private set; }

        public WUI_ErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32(255, 0, 0, 90);
        }
    }
}
