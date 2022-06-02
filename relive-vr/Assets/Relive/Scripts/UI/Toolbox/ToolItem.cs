using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Relive.Tools;
using Relive.UI.Hierarchy;
using Relive.UI.Input;
using Relive.Data;
using System;
using System.Linq;
using Relive.Sync;
using HCIKonstanz.Colibri;
using System.Collections.Generic;

namespace Relive.UI.Toolbox
{
    public class ToolItem : MonoBehaviour
    {
        public Image ToolBackgroundImage;
        public TextMeshProUGUI ToolNameText;
        public Image ToolIconImage;
        public TextMeshProUGUI ToolPropertiesText;
        public Tool Tool
        {
            get { return this.tool; }
            set
            {
                this.tool = value;
                ToolNameText.text = tool.Name;
                ToolIconImage.sprite = tool.Image;
                ToolPropertiesText.text = tool.Properties;
                LayoutRebuilder.ForceRebuildLayoutImmediate(ToolPropertiesText.GetComponent<RectTransform>());
                if (tool.MaxEntities == 0) entitiesAllowed = false;
                if (tool.MaxEvents == 0) eventsAllowed = false;
            }
        }

        public Color SelectedColor;
        public Color DeselectedColor;
        public ToolPreviewObject ToolPreviewObjectPrefab;

        private Tool tool;
        private bool selected = false;
        private GameObject toolPreviewObject;
        private LineRenderer toolPreviewLineRenderer;
        private Tool lastCreatedTool;
        private EntityGameObject lastTargetedEntity;
        private EventGameObject lastTargetedEvent;
        private GameObject lastCreatedToolGameObject;
        private bool wasPlaybackPaused = false;
        private bool entitiesAllowed = true;
        private bool eventsAllowed = true;


        void Update()
        {
            if (selected)
            {
                // Update tool preview object
                if (toolPreviewObject)
                {
                    Vector3 previewPosition = Vector3.Lerp(Laser.Instance.LaserStartPosition, Laser.Instance.LaserEndPoint, 0.66f);
                    toolPreviewObject.transform.position = previewPosition;

                    if (Laser.Instance.Hitting && IsGameObjectAllowed(Laser.Instance.RaycastHit.transform.gameObject))
                    {
                        toolPreviewObject.GetComponent<Renderer>().material.color = Laser.Instance.ValidColor;
                    }
                    else
                    {
                        toolPreviewObject.GetComponent<Renderer>().material.color = Laser.Instance.InvalidColor;
                    }
                }

                // Render line between last clicked entity and laser end 
                if (lastCreatedTool && (lastTargetedEntity || lastTargetedEvent))
                {
                    if (!toolPreviewLineRenderer)
                    {
                        toolPreviewLineRenderer = gameObject.AddComponent<LineRenderer>();
                        toolPreviewLineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                        toolPreviewLineRenderer.startWidth = 0.01f;
                        toolPreviewLineRenderer.startColor = Laser.Instance.ValidColor;
                    }

                    Vector3[] positions = new Vector3[2];
                    if (lastTargetedEntity)
                    {
                        positions[0] = lastTargetedEntity.transform.position;
                    }
                    else
                    {
                        positions[0] = lastTargetedEvent.transform.position;
                    }

                    positions[1] = Laser.Instance.LaserEndPoint;
                    toolPreviewLineRenderer.SetPositions(positions);

                    if (Laser.Instance.Hitting && IsGameObjectAllowed(Laser.Instance.RaycastHit.transform.gameObject))
                    {
                        toolPreviewLineRenderer.endColor = Laser.Instance.ValidColor;
                    }
                    else
                    {
                        toolPreviewLineRenderer.endColor = Laser.Instance.InvalidColor;
                    }
                }
            }
        }

        public void OnToolItemClicked()
        {
            if (selected)
            {
                Deselect();
            }
            else
            {
                Select();
            }
        }

        public void Select()
        {
            if (!selected)
            {
                ToolboxPanelVR.Instance.DeselectAllTools();
                selected = true;
                ToolBackgroundImage.color = SelectedColor;

                // pause playback to make selection easier
                var notebook = NotebookManager.Instance.Notebook;
                wasPlaybackPaused = notebook.IsPaused;
                notebook.IsPaused = true;
                notebook.UpdateLocal();

                // Check if tool needs any Entities or Events
                if (tool.MinEntities == 0 && tool.MaxEntities == 0 && tool.MinEvents == 0 && tool.MaxEvents == 0)
                {
                    CreateTool();
                    // HINT: I think tools with 0 entities will always be Singleton?
                    if (!tool.Singleton) Deselect();
                }
                else
                {
                    // Register event for selected objects
                    Laser.Instance.OnGameObjectSelected += OnGameObjectSelected;

                    // Create preview cube
                    toolPreviewObject = Instantiate(ToolPreviewObjectPrefab.gameObject);
                    toolPreviewObject.GetComponent<ToolPreviewObject>().SetAllSprites(tool.Image);

                    // Check if tool can already be created with 0 Entities or 0 Events
                    if ((entitiesAllowed && tool.MinEntities == 0) || (eventsAllowed && tool.MinEvents == 0))
                    {
                        CreateTool();
                    }
                }
            }
        }

        public void Deselect()
        {
            if (selected)
            {
                var notebook = NotebookManager.Instance.Notebook;
                notebook.IsPaused = wasPlaybackPaused;
                notebook.UpdateLocal();

                selected = false;
                ToolBackgroundImage.color = DeselectedColor;
                if (toolPreviewObject) Destroy(toolPreviewObject);
                if (lastCreatedTool && (lastCreatedTool.Singleton || lastCreatedTool.MinEntities > lastCreatedTool.Entities.Count || lastCreatedTool.MinEvents > lastCreatedTool.Events.Count))
                {
                    Destroy(lastCreatedTool.gameObject);
                }
                lastCreatedToolGameObject = null;
                lastTargetedEntity = null;
                lastTargetedEvent = null;
                lastCreatedTool = null;
                Laser.Instance.OnGameObjectSelected -= OnGameObjectSelected;
                if (toolPreviewLineRenderer) Destroy(toolPreviewLineRenderer);
            }
        }

        private void CreateTool()
        {
            lastCreatedToolGameObject = GameObject.Instantiate(Tool.gameObject);
            lastCreatedTool = lastCreatedToolGameObject.GetComponent<Tool>();
            lastCreatedTool.NotebookIndex = ActiveToolPanelVR.Instance.Tools.Count - 1;

            if (lastCreatedTool.Singleton)
            {
                if (!ActiveToolPanelVR.Instance.Tools.Any(t => t.Name == lastCreatedTool.Name))
                {
                    lastCreatedTool.Id = Guid.NewGuid().ToString();
                    ActiveToolPanelVR.Instance.AddTool(lastCreatedTool, true);
                }
            }
            else
            {
                lastCreatedTool.Id = Guid.NewGuid().ToString();
                ActiveToolPanelVR.Instance.AddTool(lastCreatedTool, true);
            }
        }

        private void OnGameObjectSelected(GameObject gameObject)
        {
            EntityGameObject entityGameObject;
            EventGameObject eventGameObject;

            // Entity selected
            if (entitiesAllowed && (entityGameObject = EntityGameObject.IsEntityGameObject(gameObject)))
            {
                // If the tool is not created yet (no click on a valid gameobject) create it
                if (!lastCreatedTool)
                    CreateTool();

                // Prevent to select the same entity twice (also prevent it between the sessions)
                if (lastCreatedTool.Entities.Contains(entityGameObject.Entity.name))
                    return;

                // Add the session of the entity to the tool (as an instance)
                var session = entityGameObject.PlaybackSession.LoadedSession.Session;
                lastCreatedTool.AddInstance(session, entityGameObject.PlaybackSession.SessionColor);

                // Add the entity to the tool
                lastCreatedTool.AddEntity(entityGameObject);

                // Save the entity to render the line in the Update function
                lastTargetedEntity = entityGameObject;
                lastTargetedEvent = null;

                // If max entities and events reached finish tool
                if (lastCreatedTool.MaxEntities == lastCreatedTool.Entities.Count && lastCreatedTool.MaxEvents == lastCreatedTool.Events.Count)
                {
                    Deselect();

                    // The tool remains selected until the user deselects it manually
                    Select();
                }
            }
            // Event selected
            else if (eventsAllowed && (eventGameObject = EventGameObject.IsEventGameObject(gameObject)))
            {
                // If the tool is not created yet (no click on a valid gameobject) create it
                if (!lastCreatedTool)
                    CreateTool();

                // Prevent to select the same event twice (also prevent it between the sessions if event has a name)
                if (lastCreatedTool.Events.Contains(eventGameObject.Event.name) || lastCreatedTool.Events.Contains(eventGameObject.Event.eventId))
                    return;

                // Add the session of the event to the tool (as an instance)
                var session = eventGameObject.PlaybackSession.LoadedSession.Session;
                lastCreatedTool.AddInstance(session, eventGameObject.PlaybackSession.SessionColor);

                // Add the event to the tool
                lastCreatedTool.AddEvent(eventGameObject);

                // Save the event to render the line in the Update function
                lastTargetedEvent = eventGameObject;
                lastTargetedEntity = null;

                // If max entities and events reached finish tool
                if (lastCreatedTool.MaxEntities == lastCreatedTool.Entities.Count && lastCreatedTool.MaxEvents == lastCreatedTool.Events.Count)
                {
                    Deselect();

                    // The tool remains selected until the user deselects it manually
                    Select();
                }

            }
            // UI was selected
            else if (gameObject.GetComponent<ActiveToolPanelVR>() || gameObject.GetComponent<HierarchyToolItem>())
            {
                // Abort tool selection
                Deselect();
            }
        }

        private bool IsGameObjectAllowed(GameObject gameObject)
        {
            if (entitiesAllowed && EntityGameObject.IsEntityGameObject(gameObject)) return true;
            if (eventsAllowed && EventGameObject.IsEventGameObject(gameObject)) return true;
            return false;
        }
    }
}
