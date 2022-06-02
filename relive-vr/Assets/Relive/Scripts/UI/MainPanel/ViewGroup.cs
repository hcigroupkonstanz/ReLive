using System.Collections.Generic;
using Relive.UI.Animations;
using Relive.UI.Hierarchy;
using Relive.UI.Sessions;
using Relive.UI.Toolbox;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.MainPanel
{
    public class ViewGroup : MonoBehaviour
    {
        public List<ViewButton> viewButtons;

        public ViewButton selectedView;

        public EntityPanelVR EntityPanel;
        public ToolboxPanelVR ToolboxPanel;
        public ActiveToolPanelVR ActiveToolPanel;
        public SessionPanelVR SessionPanel;
        public EventPanelVR EventPanel;

        private SlideAnimation slideAnimation;

        private void Start()
        {
            ActiveToolPanel.Deactivate();
            ResetTabs();
        }

        private void SlideOut()
        {
            slideAnimation = GetComponent<SlideAnimation>();

            if (slideAnimation)
            {
                slideAnimation.Deactivate();
            }
        }

        private void SlideIn()
        {
            slideAnimation = GetComponent<SlideAnimation>();

            if (slideAnimation)
            {
                slideAnimation.Activate();
            }
        }

        public void Subscribe(ViewButton viewButton)
        {
            if (viewButtons == null)
            {
                viewButtons = new List<ViewButton>();
            }

            viewButtons.Add(viewButton);
        }

        public void OnViewExit(ViewButton viewButton)
        {
            // Only deactivate Active Tool Panel when all feature panels are closed
            ActiveToolPanel.Deactivate();
            ResetTabs();
            SlideOut();
        }

        public void OnViewSelected(ViewButton viewButton)
        {
            if (selectedView == null)
            {
                SlideIn();
            }
            ResetTabs();
            viewButton.Toggle.SetIsOnWithoutNotify(true);
            ColorBlock cb = viewButton.Toggle.colors;
            cb.normalColor = new Color32(0, 208, 255, 255);
            cb.highlightedColor = new Color32(0, 208, 255, 255);
            cb.selectedColor = new Color32(0, 208, 255, 255);
            viewButton.Toggle.colors = cb;


            selectedView = viewButton;
            switch (viewButton.name)
            {
                case "Entities":
                    EntityPanel.Activate();
                    break;
                case "Tools":
                    ToolboxPanel.Activate();
                    break;
                case "Sessions":
                    SessionPanel.Activate();
                    break;
                case "Events":
                    EventPanel.Activate();
                    break;
                default:
                    Debug.Log("No Such Button Format");
                    break;
            }
            ActiveToolPanel.Activate();
        }

        public void ResetTabs()
        {
            selectedView = null;
            EntityPanel.Deactivate();
            ToolboxPanel.Deactivate();
            SessionPanel.Deactivate();
            EventPanel.Deactivate();

            foreach (ViewButton viewButton in viewButtons)
            {
                viewButton.Toggle.SetIsOnWithoutNotify(false);
                ColorBlock cb = viewButton.Toggle.colors;
                cb.normalColor = new Color32(255, 255, 255, 255);
                cb.highlightedColor = new Color32(255, 255, 255, 255);
                cb.selectedColor = new Color32(255, 255, 255, 255);
                viewButton.Toggle.colors = cb;
            }
        }
    }
}