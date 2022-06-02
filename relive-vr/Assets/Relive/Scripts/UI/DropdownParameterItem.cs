using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Relive.Tools.Parameter;
using UnityEngine.UI;

public class DropdownParameterItem : MonoBehaviour
{
    public TextMeshProUGUI ParameterNameText;
    // public TMP_Dropdown Dropdown;

    public Dropdown Dropdown;

    private DropdownToolParameter dropdownToolParameter;
    public DropdownToolParameter DropdownToolParameter
    {
        get { return this.dropdownToolParameter; }
        set
        {
            dropdownToolParameter = value;
            ParameterNameText.text = dropdownToolParameter.Name;
            UpdateDropdownList();
        }
    }

    private void UpdateDropdownList()
    {
        Dropdown.SetValueWithoutNotify(dropdownToolParameter.Value);
        Dropdown.ClearOptions();
        Dropdown.AddOptions(dropdownToolParameter.Options);
        Dropdown.value = dropdownToolParameter.Value;
    }

    public void OnDropdownValueChanged(int value)
    {
        dropdownToolParameter.Value = value;
    }

}
