using UnityEngine;

namespace Relive.Tools.Parameter
{
    public class ColorToolParameter : ToolParameter
    {
        public new Color Value;
        public delegate void ColorChangedHandler(Color color);
        public event ColorChangedHandler OnColorChanged;

        public void ChangeColor(Color color)
        {
            Value = color;
            OnColorChanged?.Invoke(color);
        }
    }
}
