using System.Collections.Generic;
using Relive.Data;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Relive.UI.Hierarchy
{
    public class EntityPanelVR : MonoBehaviour
    {
        public static EntityPanelVR Instance;

        public GameObject EntityContent;

        public HierarchyEntityItem HierarchyEntityItemPrefab;

        private List<HierarchyEntityItem> entityItems;
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
            entityItems = new List<HierarchyEntityItem>();
        }

        public void AddEntity(EntityGameObject entityGameObject)
        {
            // Generate hierarchy entity item
            HierarchyEntityItem hierarchyEntityItem = Instantiate(HierarchyEntityItemPrefab, EntityContent.transform);
            hierarchyEntityItem.EntityGameObject = entityGameObject;
            entityItems.Add(hierarchyEntityItem);
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                hierarchyEntityItem.ToolCountString.GetComponent<RectTransform>());
        }
    }
}