using Relive.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Hierarchy
{
    public class HierarchyToolInstanceItem : MonoBehaviour
    {
        [HideInInspector]
        public ToolInstance ToolInstance;

        public Image InstanceColorImage;
        public TextMeshProUGUI ResultText;
        public TextMeshProUGUI SessionText;

        private void Update()
        {
            if (ToolInstance != null)
            {
                InstanceColorImage.color = ToolInstance.Color;
                SessionText.text = ToolInstance.Session.name;
                ResultText.text = ToolInstance.GetResult();
                Color32 color = (Color32) ToolInstance.Color;

                //Color result text white or black depending on background
                if ((color.r * 0.299f + color.g * 0.587f + color.b * 0.114f) > 186) {
                    ResultText.color = new Color(0, 0, 0, 255);
                    SessionText.color = new Color(0, 0, 0, 255);
                } else {
                    ResultText.color = new Color(255, 255, 255, 255);
                    SessionText.color = new Color(255, 255, 255, 255);
                }

            }
        }

        public void OnToggleChanged()
        {
            if (ToolInstance != null)
            {
                ActiveToolPanelVR.Instance.PropertyWindowActivated(this);
            }
        }
    }
}
