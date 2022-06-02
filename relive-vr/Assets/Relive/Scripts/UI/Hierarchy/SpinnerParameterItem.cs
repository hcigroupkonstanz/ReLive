using UnityEngine;
using TMPro;
using Relive.Tools.Parameter;

namespace Relive.UI.Hierarchy
{
    public class SpinnerParameterItem : MonoBehaviour
    {
        public TextMeshProUGUI ParameterNameText;
        public TextMeshProUGUI SpinnerValueText;

        private NumberToolParameter numberToolParameter;
        public NumberToolParameter NumberToolParameter
        {
            get { return this.numberToolParameter; }
            set
            {
                numberToolParameter = value;
                ParameterNameText.text = numberToolParameter.Name;
                numberToolParameter.OnValueChanged += OnValueChanged;
                UpdateSpinnerValueText();
            }
        }

        public void OnValueChanged(float value)
        {
            UpdateSpinnerValueText();
        }

        public void OnMinButtonClicked()
        {
            numberToolParameter.SetToMin();
        }

        public void OnSmallerButtonClicked()
        {
            numberToolParameter.Decrease();
        }

        public void OnHigherButtonClicked()
        {
            numberToolParameter.Increase();
        }

        public void OnMaxButtonClicked()
        {
            numberToolParameter.SetToMax();
        }

        private void UpdateSpinnerValueText()
        {
            SpinnerValueText.text = numberToolParameter.Value + " " + numberToolParameter.Unit;
        }
    }
}
