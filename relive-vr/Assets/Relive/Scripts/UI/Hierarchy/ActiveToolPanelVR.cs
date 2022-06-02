using System.Collections.Generic;
using System.Linq;
using HCIKonstanz.Colibri;
using Relive.Data;
using Relive.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Hierarchy
{
    public class ActiveToolPanelVR : MonoBehaviour
    {
        public static ActiveToolPanelVR Instance;

        public HierarchyToolItem HierarchyToolItemPrefab2;

        public GameObject ToolContent;
        private GameObject activeTool;
        public PropertyWindow PropertyWindow;

        public CanvasGroup PropertyWindowCanvasGroup;
        public CanvasGroup ActiveToolsCanvasGroup;

        public readonly List<Tool> Tools = new List<Tool>();
        public readonly List<HierarchyToolItem> UiTools = new List<HierarchyToolItem>();

        private LayoutElement layoutElement;

        public delegate void ToolEvent(Tool tool);
        public event ToolEvent OnToolAdded;
        public event ToolEvent OnToolDeleted;

        private void Awake()
        {
            Instance = this;
            layoutElement = GetComponent<LayoutElement>();
        }

        public void ActivatePropertyWindow()
        {
            PropertyWindowCanvasGroup.interactable = true;
            PropertyWindowCanvasGroup.blocksRaycasts = true;
            PropertyWindowCanvasGroup.alpha = 1;
            BoxCollider[] BoxColliders = PropertyWindowCanvasGroup.gameObject.GetComponentsInChildren<BoxCollider>();
            BoxColliders = BoxColliders.Concat(GetComponents<BoxCollider>()).ToArray();
            foreach (BoxCollider collider in BoxColliders)
            {
                collider.enabled = true;
            }
        }

        public void ActivateActiveToolWindow()
        {
            ActiveToolsCanvasGroup.interactable = true;
            ActiveToolsCanvasGroup.blocksRaycasts = true;
            ActiveToolsCanvasGroup.alpha = 1;
            BoxCollider[] BoxColliders = ActiveToolsCanvasGroup.gameObject.GetComponentsInChildren<BoxCollider>();
            BoxColliders = BoxColliders.Concat(GetComponents<BoxCollider>()).ToArray();
            foreach (BoxCollider collider in BoxColliders)
            {
                collider.enabled = true;
            }
        }

        public void DeactivatePropertyWindow()
        {
            PropertyWindowCanvasGroup.interactable = false;
            PropertyWindowCanvasGroup.blocksRaycasts = false;
            PropertyWindowCanvasGroup.alpha = 0;
            BoxCollider[] BoxColliders = PropertyWindowCanvasGroup.gameObject.GetComponentsInChildren<BoxCollider>();
            BoxColliders = BoxColliders.Concat(GetComponents<BoxCollider>()).ToArray();
            foreach (BoxCollider collider in BoxColliders)
            {
                collider.enabled = false;
            }
        }

        public void DeactivateActiveToolWindow()
        {
            ActiveToolsCanvasGroup.interactable = false;
            ActiveToolsCanvasGroup.blocksRaycasts = false;
            ActiveToolsCanvasGroup.alpha = 0;
            BoxCollider[] BoxColliders = ActiveToolsCanvasGroup.gameObject.GetComponentsInChildren<BoxCollider>();
            BoxColliders = BoxColliders.Concat(GetComponents<BoxCollider>()).ToArray();
            foreach (BoxCollider collider in BoxColliders)
            {
                collider.enabled = false;
                print("Collider deactivate: " + collider.gameObject.name);
            }
        }

        public void Deactivate()
        {
            DeactivatePropertyWindow();
            DeactivateActiveToolWindow();

            if (activeTool)
            {
                activeTool.GetComponent<Toggle>().SetIsOnWithoutNotify(false);
                activeTool.GetComponent<HierarchyToolToggle>().Controls.SetActive(false);
                activeTool.GetComponent<HierarchyToolItem>().RemoveOutlineFromSlots();
                activeTool = null;
            }

            layoutElement.ignoreLayout = true;
        }

        public void Activate()
        {
            // Start with Property Window deactivated
            DeactivatePropertyWindow();
            ActivateActiveToolWindow();

            layoutElement.ignoreLayout = false;
        }

        public void AddTool(Tool tool, bool local)
        {
            HierarchyToolItem hierarchyToolItemPrefab2 = Instantiate(HierarchyToolItemPrefab2, ToolContent.transform);
            hierarchyToolItemPrefab2.Tool = tool;

            hierarchyToolItemPrefab2.IsSingleton = tool.Singleton;
            // FIXME: workaround for deleting camera tool from webinterface...
            hierarchyToolItemPrefab2.HackName = tool.Name;

            Tools.Add(tool);
            UiTools.Add(hierarchyToolItemPrefab2);

            OnToolAdded?.Invoke(tool);
        }

        public void ToggleValueChanged(HierarchyToolToggle hierarchyToolToggle)
        {
            Toggle pressedToggle = hierarchyToolToggle.GetComponent<Toggle>();

            if (pressedToggle.isOn)
            {
                hierarchyToolToggle.SlideOut();

                if (activeTool)
                {
                    Toggle currentToggle = activeTool.GetComponent<Toggle>();
                    HierarchyToolToggle currentHierarchyToolToggle = activeTool.GetComponent<HierarchyToolToggle>();
                    activeTool.GetComponent<HierarchyToolItem>().RemoveOutlineFromSlots();
                    if (currentToggle.isOn)
                    {
                        if (!PropertyWindow.gameObject.activeSelf)
                        {
                            currentHierarchyToolToggle.SlideIn();
                        }

                        currentToggle.SetIsOnWithoutNotify(false);
                    }
                }

                DeactivatePropertyWindow();
                activeTool = hierarchyToolToggle.gameObject;
                activeTool.GetComponent<HierarchyToolItem>().AddOutlineToSlots();

            }
            else
            {
                if (activeTool)
                    activeTool.GetComponent<HierarchyToolItem>().RemoveOutlineFromSlots();
                activeTool = null;
                hierarchyToolToggle.SlideIn();
            }
        }

        public void PropertyWindowActivated(HierarchyToolItem activeItem)
        {
            DeactivateActiveToolWindow();
            ActivatePropertyWindow();
            PropertyWindow.SetParameterWindow(activeItem);
            activeTool.GetComponent<HierarchyToolToggle>().SlideIn();
            Toggle activeToggle = activeItem.GetComponent<Toggle>();
            activeToggle.SetIsOnWithoutNotify(false);
        }

        public void PropertyWindowActivated(HierarchyToolInstanceItem activeInstanceItem)
        {
            DeactivateActiveToolWindow();
            ActivatePropertyWindow();
            PropertyWindow.SetParameterWindow(activeInstanceItem);
        }


        public void OnPropertyWindowBackButtonTapped()
        {
            if (activeTool) activeTool.GetComponent<HierarchyToolItem>().RemoveOutlineFromSlots();
            activeTool = null;
            DeactivatePropertyWindow();
            ActivateActiveToolWindow();
        }


        public void DeleteTool(string id)
        {
            var tool = UiTools.FirstOrDefault(t => t.Tool.Id == id);
            if (tool != null)
                DeleteTool(tool);
        }

        public void DeleteTool(HierarchyToolItem activeItem)
        {
            activeItem.GetComponent<HierarchyToolToggle>().SlideIn();
            activeItem.Delete();
            if (activeTool)
                activeTool.GetComponent<HierarchyToolItem>().RemoveOutlineFromSlots();
            activeTool = null;

            Tools.Remove(activeItem.Tool);
            OnToolDeleted?.Invoke(activeItem.Tool);
        }

        public void DetachTool(HierarchyToolItem activeItem)
        {
            activeItem.GetComponent<HierarchyToolToggle>().SlideIn();
            activeItem.InstantiateWorldSpaceTool();
        }

        public void DeleteTools()
        {
            foreach (HierarchyToolItem activeItem in UiTools)
            {
                if (activeItem)
                    DeleteTool(activeItem);
            }
            UiTools.Clear();
        }

        public void CheckForDuplicates(HierarchyToolItem testItem)
        {
            bool isDuplicate = false;

            foreach (HierarchyToolItem child in ToolContent.GetComponentsInChildren<HierarchyToolItem>())
            {
                if (!testItem.Equals(child) && testItem.Tool.Name.Equals(child.Tool.Name))
                {
                    isDuplicate = true;

                    foreach (var item in testItem.Tool.Instances)
                    {
                        var instance = child.Tool.Instances.FirstOrDefault(e => e.Session.sessionId == item.Session.sessionId);
                        if (instance == null)
                        {
                            isDuplicate = false;
                        }
                    }

                    foreach (var item in testItem.Tool.Entities)
                    {
                        int pos = child.Tool.Entities.IndexOf(item);
                        if (pos <= -1)
                        {
                            isDuplicate = false;
                        }
                    }

                    foreach (var item in testItem.Tool.Events)
                    {
                        int pos = child.Tool.Events.IndexOf(item);
                        if (pos <= -1)
                        {
                            isDuplicate = false;
                        }
                    }

                    if (isDuplicate) break;
                }
            }

            if (isDuplicate)
            {
                testItem.Delete();
            }
        }

        public void ReorderTools()
        {
            foreach (HierarchyToolItem child in ToolContent.GetComponentsInChildren<HierarchyToolItem>())
            {
                child.transform.SetSiblingIndex(child.Tool.NotebookIndex + 1);
            }
        }
    }
}