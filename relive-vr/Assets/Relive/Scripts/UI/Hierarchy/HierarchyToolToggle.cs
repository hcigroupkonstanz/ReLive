using System;
using Relive.UI.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Hierarchy
{
    public class HierarchyToolToggle : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject Controls;
        public Canvas UnmaskedCanvas;

        private SlideX slideAnimation;

        private void Start()
        {
            slideAnimation = GetComponent<SlideX>();
            UnmaskedCanvas = GetComponentInParent<Canvas>();

            Toggle toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(
                delegate { ActiveToolPanelVR.Instance.ToggleValueChanged(this); });
        }

        public void SlideOut()
        {
            slideAnimation.Activate();
        }

        public void SlideIn()
        {
            slideAnimation.Deactivate();
        }
    }
}