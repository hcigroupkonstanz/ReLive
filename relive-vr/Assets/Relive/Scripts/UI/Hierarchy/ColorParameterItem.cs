using System.Collections;
using System.Collections.Generic;
using Relive.Tools.Parameter;
using TMPro;
using UnityEngine;

public class ColorParameterItem : MonoBehaviour
{
    public TextMeshProUGUI ParameterNameText;
    public ColorPicker ColorPicker;

    private ColorToolParameter colorToolParameter;
    public ColorToolParameter ColorToolParameter
    {
        get { return this.colorToolParameter; }
        set
        {
            colorToolParameter = value;
            ParameterNameText.text = colorToolParameter.Name;
            ColorPicker.AssignColor(colorToolParameter.Value);
            colorToolParameter.OnColorChanged += OnColorChanged;
        }
    }

    public void OnColorValueChanged(Color color)
    {
        colorToolParameter.ChangeColor(color);
    }

    public void OnColorChanged(Color color)
    {
        ColorPicker.AssignColor(color);
    }
}
