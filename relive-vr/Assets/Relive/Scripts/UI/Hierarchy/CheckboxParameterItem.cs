using Relive.Tools;
using Relive.Tools.Parameter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Hierarchy
{
    public class CheckboxParameterItem : MonoBehaviour
    {
        public Toggle Toggle;

        public TextMeshProUGUI ParameterNameText;

        private BoolToolParameter boolToolParameter;
        public BoolToolParameter BoolToolParameter
        {
            get { return this.boolToolParameter; }
            set
            {
                boolToolParameter = value;
                ParameterNameText.text = boolToolParameter.Name;
                Toggle.SetIsOnWithoutNotify(boolToolParameter.Value);
                // Toggle.isOn = boolToolParameter.Value;
                boolToolParameter.OnValueChanged += OnValueChanged;
            }
        }

        public void OnValueChanged(BoolToolParameter value)
        {
            Toggle.SetIsOnWithoutNotify(value.Value);
        }

        public void OnToggleValueChanged(bool value)
        {
            BoolToolParameter.ChangeValue(value);
        }
    }
}
