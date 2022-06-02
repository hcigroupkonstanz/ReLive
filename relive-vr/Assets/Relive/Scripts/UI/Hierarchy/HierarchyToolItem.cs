using System.Collections.Generic;
using System.Linq;
using Relive.Data;
using Relive.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Relive.UI.Hierarchy
{
    public class HierarchyToolItem : MonoBehaviour
    {
        public TextMeshProUGUI ToolName;
        public TextMeshProUGUI EntitiesEventsTitle;
        public Image ToolImage;
        public GameObject Slots;
        public ToolEntityItem SlotPrefab;
        public WorldSpaceTool WorldSpaceToolPrefab;

        public Transform InstanceList;
        public HierarchyToolInstanceItem InstanceTemplate;
        private List<HierarchyToolInstanceItem> currentInstanceEntries = new List<HierarchyToolInstanceItem>();

        public bool IsSingleton;
        // FIXME: workaround for syncing camera with webinterface..
        public string HackName;

        private Tool tool;
        private bool toolWasSet = false;
        private List<ToolEntityItem> toolSlotItems;
        private bool slotsFilled;
        public WorldSpaceTool WorldSpaceTool;
        public RectTransform Description;

        private bool showOutlines;

        const int TOOL_HEIGHT = 60;
        const int INSTANCE_HEIGHT = 30;
        const int PADDING_BOTTOM = 10;

        public Tool Tool
        {
            get { return tool; }
            set
            {
                tool = value;
                toolWasSet = true;
                ToolName.text = tool.Name;
                ToolImage.sprite = tool.Image;
                if (tool.MaxEntities == 0 && tool.MaxEvents > 0)
                {
                    EntitiesEventsTitle.text = "Events:";
                }
                else if (tool.MaxEvents == 0 && tool.MaxEntities > 0)
                {
                    EntitiesEventsTitle.text = "Entities:";
                }
                else
                {
                    EntitiesEventsTitle.text = "Entities/Events:";
                }

                tool.InstanceAdded += OnToolInstanceAdded;
                tool.InstanceRemoved += OnToolInstanceRemoved;

                // set up instances list
                foreach (var instance in tool.Instances)
                    OnToolInstanceAdded(instance);
            }
        }

        public void AddOutlineToSlots()
        {
            showOutlines = true;
        }

        public void RemoveOutlineFromSlots()
        {
            showOutlines = false;

            foreach (var instance in tool.Instances)
            {
                foreach (var entityName in tool.Entities)
                {
                    var entityGameObject = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == entityName && e.Entity.sessionId == instance.Session.sessionId);
                    entityGameObject?.HideOutline();
                }
            }
        }

        private void OnToolInstanceAdded(ToolInstance instance)
        {
            // GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, TOOL_HEIGHT + INSTANCE_HEIGHT * tool.Instances.Count + PADDING_BOTTOM);
            var entry = Instantiate(InstanceTemplate, InstanceList);
            entry.ToolInstance = instance;
            currentInstanceEntries.Add(entry);
            // float height = Description.sizeDelta.y + 15f;
            // if (height < 60f) height = 60f;
            // GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void OnToolInstanceRemoved(ToolInstance instance)
        {
            // GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, TOOL_HEIGHT + INSTANCE_HEIGHT * tool.Instances.Count + PADDING_BOTTOM);

            var entry = currentInstanceEntries.FirstOrDefault(e => e.ToolInstance == instance);
            if (entry)
            {
                Destroy(entry.gameObject);
                currentInstanceEntries.Remove(entry);
            }
        }


        private void Update()
        {
            if (tool != null)
            {
                // Remove any invalid entry

                // add new instance entries


                if (showOutlines)
                {
                    // Call this every frame when active to update color value if changed
                    foreach (var instance in tool.Instances)
                    {
                        foreach (var entityName in tool.Entities)
                        {
                            var entityGameObject = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == entityName && e.Entity.sessionId == instance.Session.sessionId);
                            entityGameObject?.ShowOutline(instance.Color);
                        }
                        foreach (var eventName in tool.Events)
                        {
                            var eventGameObject = EventGameObject.ActiveEvents.FirstOrDefault(e => (e.Event.name == eventName || e.Event.eventId == eventName) && e.Event.sessionId == instance.Session.sessionId);
                            eventGameObject?.ShowOutline(instance.Color);
                        }
                    }
                }

                if (!slotsFilled && !tool.Singleton)
                {
                    // Create slots for entities
                    if (toolSlotItems == null && (tool.MinEntities > 0 || tool.MinEvents > 0))
                    {
                        toolSlotItems = new List<ToolEntityItem>();
                        for (int i = 0; i < (tool.MinEntities + tool.MinEvents); i++)
                        {
                            ToolEntityItem toolSlotItem = Instantiate(SlotPrefab, Slots.transform);
                            toolSlotItems.Add(toolSlotItem);
                        }
                    }

                    bool isFilled = true;
                    // Fill slots
                    for (int i = 0; i < toolSlotItems.Count; i++)
                    {
                        if (i < tool.Entities.Count && tool.Entities[i] != null)
                        {
                            toolSlotItems[i].Name = tool.Entities[i];
                        }
                        else if (i < (tool.Entities.Count + tool.Events.Count) && tool.Events[i - tool.Entities.Count] != null)
                        {
                            toolSlotItems[i].Name = tool.Events[i - tool.Entities.Count];
                        }
                        else
                        {
                            toolSlotItems[i].Name = null;
                            isFilled = false;
                        }

                        LayoutRebuilder.ForceRebuildLayoutImmediate(toolSlotItems[i].GetComponent<RectTransform>());
                    }

                    slotsFilled = isFilled;

                    if (slotsFilled)
                        ActiveToolPanelVR.Instance.CheckForDuplicates(this);
                }
            }
            else if (toolWasSet && !IsSingleton)
            {
                Delete();
            }
        }

        private void ClearSlots()
        {
            if (toolSlotItems != null)
            {
                foreach (var toolSlotItem in toolSlotItems)
                    Destroy(toolSlotItem.gameObject);
            }
        }

        public void OnDeleteButtonClicked()
        {
            Delete();
        }

        public void Delete()
        {
            ClearSlots();
            if (tool != null)
                Destroy(tool.gameObject);
            Destroy(gameObject);
        }

        public void InstantiateWorldSpaceTool()
        {
            // only have one copy of the tool in the scene
            if (!WorldSpaceTool)
            {
                WorldSpaceTool = Instantiate(WorldSpaceToolPrefab, null);
                //with the correct size transform
                Canvas canvas = WorldSpaceTool.gameObject.AddComponent<Canvas>();
                Canvas toolCanvas = GetComponentInParent<Canvas>();
                // Get camera of tool canvas
                canvas.worldCamera = toolCanvas.worldCamera;
                WorldSpaceTool.gameObject.AddComponent<CanvasScaler>();
                WorldSpaceTool.gameObject.AddComponent<GraphicRaycaster>();
                WorldSpaceTool.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                RectTransform hierarchySize = GetComponent<RectTransform>();
                RectTransform worldSpaceToolRectTransform = WorldSpaceTool.GetComponent<RectTransform>();
                worldSpaceToolRectTransform.sizeDelta =
                    new Vector2(hierarchySize.rect.width, hierarchySize.rect.height);
                worldSpaceToolRectTransform.localScale = toolCanvas.GetComponent<RectTransform>().localScale;

                // move it to the position of the control buttons of the hierarchy element
                worldSpaceToolRectTransform.position = GetComponentInChildren<ActiveToolControlButtons>().transform.position;
                worldSpaceToolRectTransform.rotation = GetComponentInChildren<ActiveToolControlButtons>().transform.rotation;

                WorldSpaceTool.HierarchyToolItem = this;
            }
        }
    }
}