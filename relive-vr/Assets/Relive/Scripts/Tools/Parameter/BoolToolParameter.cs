namespace Relive.Tools.Parameter
{
    public class BoolToolParameter : ToolParameter
    {
        public new bool Value;
        
        public delegate void BoolChangedHandler(BoolToolParameter value);

        public event BoolChangedHandler OnValueChanged;
        
        public void ChangeValue(bool value)
        {
            Value = value;
            OnValueChanged?.Invoke(this);
        }
    }
}
