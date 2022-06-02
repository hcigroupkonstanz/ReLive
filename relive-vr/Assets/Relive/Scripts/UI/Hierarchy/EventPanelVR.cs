using System.Collections.Generic;
using Relive.Data;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Relive.UI.Hierarchy
{
    public class EventPanelVR : MonoBehaviour
    {
        public static EventPanelVR Instance;

        public GameObject EventContent;

        public EventItem EventItemPrefab;

        private List<EventItem> eventItems;

        private LayoutElement layoutElement;

        void Awake()
        {
            Instance = this;
            layoutElement = GetComponent<LayoutElement>();
        }

        public void Deactivate()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            BoxCollider[] BoxColliders = GetComponentsInChildren<BoxCollider>();
            BoxColliders.Concat(GetComponents<BoxCollider>());
            foreach (BoxCollider collider in BoxColliders)
            {
                collider.enabled = false;
            }

            layoutElement.ignoreLayout = true;
        }

        public void Activate()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1;

            BoxCollider[] BoxColliders = GetComponentsInChildren<BoxCollider>();
            BoxColliders.Concat(GetComponents<BoxCollider>());
            foreach (BoxCollider collider in BoxColliders)
            {
                collider.enabled = true;
            }

            layoutElement.ignoreLayout = false;
        }

        private void Start()
        {
            eventItems = new List<EventItem>();
        }

        public void AddEvent(EventGameObject eventGameObject)
        {
            // Generate event item
            EventItem eventItem = Instantiate(EventItemPrefab, EventContent.transform);
            eventItem.EventGameObject = eventGameObject;
            eventItems.Add(eventItem);
            LayoutRebuilder.ForceRebuildLayoutImmediate(eventItem.ToolCountString.GetComponent<RectTransform>());
        }
    }
}