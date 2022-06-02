using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Relive.Data;
using Relive.Visualizations;
using UnityEngine;

namespace Relive.Tools
{
    public class EventTimerTool : Tool
    {

        private class EventTimerToolInstance : ToolInstance
        {
            public long timeBetweenEventsMillis = 0;
            public FastLineRenderer fastLineRenderer;
            public Vector3[] positions = new Vector3[2];
            public Color[] colors = new Color[2];

            public ToolValueBadge toolValueBadge;
            public override string GetResult()
            {
                return TimeSpan.FromMilliseconds(timeBetweenEventsMillis).ToString(@"mm\:ss\:fff");
            }
        }

        [Header("Custom Properties")]
        public ToolValueBadge ToolValueBadgePrefab;
        public FastLineRenderer LineRendererPrefab;

        private void Update()
        {
            if (RenderVisualization && Events.Count == 2 && Events[0] != null && Events[1] != null)
            {
                foreach (EventTimerToolInstance instance in Instances)
                    UpdateInstance(instance);
            }
            else
            {
                foreach (EventTimerToolInstance instance in Instances)
                {
                    instance.fastLineRenderer.enabled = false;
                    instance.toolValueBadge?.gameObject.SetActive(false);
                    instance.IsActive = false;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (EventTimerToolInstance instance in Instances)
            {
                if (instance.toolValueBadge)
                    Destroy(instance.toolValueBadge.gameObject);
            }
            Destroy(gameObject);
        }


        private void UpdateInstance(EventTimerToolInstance instance)
        {
            if (instance.IsActive && !instance.Session.IsActive)
            {
                instance.toolValueBadge.gameObject.SetActive(false);
                instance.fastLineRenderer.enabled = false;
                instance.IsActive = false;
            }
            else if (!instance.IsActive && instance.Session.IsActive)
            {
                if (instance.toolValueBadge == null)
                    instance.toolValueBadge = Instantiate(ToolValueBadgePrefab);
                else
                    instance.toolValueBadge.gameObject.SetActive(true);
                instance.toolValueBadge.Color = instance.Color;
                instance.fastLineRenderer.enabled = true;
                instance.IsActive = true;
            }

            var sessionEvents = EventGameObject.ActiveEvents.Where(e => e.Event.sessionId == instance.Session.sessionId);
            var event0 = sessionEvents.FirstOrDefault(e => (e.Event.name == Events[0] || e.Event.eventId == Events[0]));
            var event1 = sessionEvents.FirstOrDefault(e => (e.Event.name == Events[1] || e.Event.eventId == Events[1]));

            // Render line
            if (event0 && event1)
            {
                instance.positions[0] = event0.transform.position;
                instance.positions[1] = event1.transform.position;
                instance.fastLineRenderer.SetPoints(instance.positions, instance.colors);
                // fastLineRenderer.ShowLineSegment(0, 1);

                // Calculate time between events
                instance.timeBetweenEventsMillis = Math.Abs(event1.Event.timestamp - event0.Event.timestamp);

                // Update tool badge
                instance.toolValueBadge.Position = Vector3.Lerp(event0.transform.position, event1.transform.position, 0.5f);
                instance.toolValueBadge.Value = instance.GetResult();
            }
        }

        public override void AddEvent(string eventName)
        {
            if (Events.Count < MaxEvents && !Events.Contains(eventName))
                Events.Add(eventName);
        }

        public override bool AddEvent(EventGameObject eventGameObject)
        {
            if (Events.Count < MaxEvents)
            {
                string name;
                if (eventGameObject.Event.name != null)
                {
                    name = eventGameObject.Event.name;
                }
                else
                {
                    name = eventGameObject.Event.eventId;
                }
                Events.Add(name);
                return true;
            }
            return false;
        }

        public override void RemoveEvent(string eventName)
        {
            Events.Remove(eventName);
        }

        public override void AddInstance(Session session, Color color)
        {
            if (Instances.Any(i => i.Session == session))
                return;

            var instance = new EventTimerToolInstance
            {
                Session = session,
                Color = color,
                fastLineRenderer = Instantiate(LineRendererPrefab, transform)
            };

            instance.OnColorUpdate += OnColorChanged;
            OnColorChanged(instance);
            Instances.Add(instance);

            InvokeInstanceAdded(instance);
        }

        public override void RemoveInstance(Session session)
        {
            var toolInstance = Instances.FirstOrDefault(t => t.Session == session) as EventTimerToolInstance;
            if (toolInstance != null)
            {
                toolInstance.OnColorUpdate -= OnColorChanged;
                Instances.Remove(toolInstance);

                InvokeInstanceRemoved(toolInstance);
            }
        }

        public void OnColorChanged(ToolInstance instance)
        {
            if (instance is EventTimerToolInstance eti)
            {
                eti.colors[0] = eti.Color;
                eti.colors[1] = eti.Color;
                if (eti.toolValueBadge != null)
                    eti.toolValueBadge.Color = eti.Color;
            }
        }
        public override void AddEntity(string entityName)
        {

        }

        public override bool AddEntity(EntityGameObject entityGameObject)
        {
            return false;
        }

        public override void RemoveEntity(string entityName)
        {

        }
    }
}
