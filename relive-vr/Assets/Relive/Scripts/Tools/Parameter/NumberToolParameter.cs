namespace Relive.Tools.Parameter
{
    public class NumberToolParameter : ToolParameter
    {
        public new float Value;
        public float MinValue = 0f;
        public float MaxValue = 100f;
        public float StepSize = 1f;
        public string Unit = "";

        public delegate void NumberChangedHandler(float value);

        public event NumberChangedHandler OnValueChanged;

        public void ChangeValue(float value)
        {
            Value = value;
            OnValueChanged?.Invoke(Value);
        }

        public void SetToMin()
        {
            Value = MinValue;
            OnValueChanged?.Invoke(Value);
        }

        public void Decrease()
        {
            if (Value - StepSize >= MinValue)
            {
                Value -= StepSize;
            }
            else
            {
                Value = MinValue;
            }

            OnValueChanged?.Invoke(Value);
        }

        public void Increase()
        {
            if (Value + StepSize <= MaxValue)
            {
                Value += StepSize;
            }
            else
            {
                Value = MaxValue;
            }

            OnValueChanged?.Invoke(Value);
        }

        public void SetToMax()
        {
            Value = MaxValue;
            OnValueChanged?.Invoke(Value);
        }
    }
}