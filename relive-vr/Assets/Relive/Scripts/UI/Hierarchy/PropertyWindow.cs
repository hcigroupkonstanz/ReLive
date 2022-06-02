using System;
using Relive.Tools.Parameter;
using TMPro;
using UnityEngine;

namespace Relive.UI.Hierarchy
{
    public class PropertyWindow : MonoBehaviour
    {
        public TextMeshProUGUI HeaderText;
        public GameObject Properties;
        public CheckboxParameterItem CheckboxParameterItemPrefab;
        public SpinnerParameterItem SpinnerParameterItemPrefab;
        public ColorParameterItem ColorParameterItemPrefab;

        private HierarchyToolItem activeToolItem;

        public void ResetParameterWindow()
        {
            foreach (Transform child in Properties.transform)
            {
                Destroy(child.gameObject);
            }
            activeToolItem = null;
            HeaderText.text = String.Empty;
        }

        public void SetParameterWindow(HierarchyToolItem toolItem)
        {
            // if (activeToolItem != null)
            ResetParameterWindow();

            activeToolItem = toolItem;

            HeaderText.text = activeToolItem.ToolName.text; // + " — Settings";

            foreach (ToolParameter toolParameter in toolItem.Tool.Parameters.Values)
            {
                if (toolParameter is BoolToolParameter)
                {
                    CheckboxParameterItem checkboxParameterItem = Instantiate(CheckboxParameterItemPrefab, Properties.transform);
                    checkboxParameterItem.BoolToolParameter = (BoolToolParameter) toolParameter;
                }
                else if (toolParameter is NumberToolParameter)
                {
                    SpinnerParameterItem spinnerParameterItem = Instantiate(SpinnerParameterItemPrefab, Properties.transform);
                    spinnerParameterItem.NumberToolParameter = (NumberToolParameter) toolParameter;
                }
                else if (toolParameter is ColorToolParameter)
                {
                    ColorParameterItem colorParameterItem = Instantiate(ColorParameterItemPrefab, Properties.transform);
                    colorParameterItem.ColorToolParameter = (ColorToolParameter) toolParameter;
                }
                else if (false)
                {
                    // TODO: Other Parameter variants
                }
            }
        }

        public void SetParameterWindow(HierarchyToolInstanceItem toolInstanceItem)
        {
            ResetParameterWindow();

            HeaderText.text = toolInstanceItem.ToolInstance.Session.name;

            ColorToolParameter colorToolParameter = new ColorToolParameter();
            colorToolParameter.Name = "Color";
            colorToolParameter.Value = toolInstanceItem.ToolInstance.Color;
            colorToolParameter.OnColorChanged += (Color color) => {
                toolInstanceItem.ToolInstance.Color = color;
                toolInstanceItem.ToolInstance.TriggerColorUpdate();
            };
            ColorParameterItem colorParameterItem = Instantiate(ColorParameterItemPrefab, Properties.transform);
            colorParameterItem.ColorToolParameter = colorToolParameter;
        }
    }
}