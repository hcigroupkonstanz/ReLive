using System.Collections.Generic;
using System.Linq;
using Relive.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Toolbox
{
    public class ToolboxPanelVR : MonoBehaviour
    {
        public static ToolboxPanelVR Instance;
        public GameObject ToolContent;
        public ToolItem ToolItemPrefab;
        // public List<Tool> Tools;
        private LayoutElement layoutElement;
        private List<ToolItem> toolItems;

        void Awake()
        {
            Instance = this;
            layoutElement = GetComponent<LayoutElement>();
        }

        void Start()
        {
            toolItems = new List<ToolItem>();

            // Create tools in toolbox
            foreach (Tool tool in Toolbox.Instance.Tools)
            {
                ToolItem toolItem = Instantiate(ToolItemPrefab, ToolContent.transform);
                toolItem.Tool = tool;
                toolItem.gameObject.name = tool.Name;
                toolItems.Add(toolItem);
            }
        }

        public void Deactivate()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            BoxCollider[] boxColliders = GetComponentsInChildren<BoxCollider>();
            boxColliders.Concat(GetComponents<BoxCollider>());
            foreach (BoxCollider collider in boxColliders)
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
            BoxCollider[] boxColliders = GetComponentsInChildren<BoxCollider>();
            boxColliders.Concat(GetComponents<BoxCollider>());
            foreach (BoxCollider collider in boxColliders)
            {
                collider.enabled = true;
            }

            layoutElement.ignoreLayout = false;
        }

        public void DeselectAllTools()
        {
            foreach (ToolItem toolItem in toolItems)
            {
                toolItem.Deselect();
            }
        }
    }
}