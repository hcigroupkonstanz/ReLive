using UnityEngine;
using Relive.Data;
using Relive.Tools.Parameter;
using Relive.Visualizations;
using System.Linq;

namespace Relive.Tools
{
    public class MeasuringTool : Tool
    {
        private class MeasuringToolInstance : ToolInstance
        {
            public float distance = 0.0f;
            public FastLineRenderer fastLineRenderer;
            public Vector3[] positions = new Vector3[2];
            public Color[] colors = new Color[2];

            public ToolValueBadge toolValueBadge;

            public override string GetResult()
            {
                return distance.ToString("f2") + "m";
            }
        }


        [Header("Custom Properties")]
        public ToolValueBadge ToolValueBadgePrefab;
        public FastLineRenderer LineRendererPrefab;

        public void OnColorChanged(ToolInstance instance)
        {
            if (instance is MeasuringToolInstance mti)
            {
                mti.colors[0] = mti.Color;
                mti.colors[1] = mti.Color;
                if (mti.toolValueBadge != null)
                    mti.toolValueBadge.Color = mti.Color;
            }
        }

        public override void AddEntity(string entityName)
        {
            if (Entities.Count < MaxEntities && !Entities.Contains(entityName))
                Entities.Add(entityName);
        }

        public override void RemoveEntity(string entityName)
        {
            Entities.Remove(entityName);
        }

        public override bool AddEntity(EntityGameObject entityGameObject)
        {
            if (Entities.Count < MaxEntities)
            {
                Entities.Add(entityGameObject.name);
                return true;
            }
            return false;
        }


        public override void AddInstance(Session session, Color color)
        {
            if (Instances.Any(i => i.Session == session))
                return;

            var instance = new MeasuringToolInstance
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
            var toolInstance = Instances.FirstOrDefault(t => t.Session == session) as MeasuringToolInstance;
            if (toolInstance != null)
            {
                toolInstance.OnColorUpdate -= OnColorChanged;
                Instances.Remove(toolInstance);
                Destroy(toolInstance.fastLineRenderer.gameObject);

                InvokeInstanceRemoved(toolInstance);
            }
        }

        private void OnDestroy()
        {
            foreach (MeasuringToolInstance instance in Instances)
            {
                if (instance.toolValueBadge)
                    Destroy(instance.toolValueBadge.gameObject);
            }
            Destroy(gameObject);
        }

        private void Update()
        {
            if (RenderVisualization && Entities.Count == 2 && Entities[0] != null && Entities[1] != null)
            {
                foreach (MeasuringToolInstance instance in Instances)
                    UpdateInstance(instance);
            }
            else
            {
                foreach (MeasuringToolInstance instance in Instances)
                {
                    instance.fastLineRenderer.enabled = false;
                    instance.toolValueBadge?.gameObject.SetActive(false);
                    instance.IsActive = false;
                }
            }
        }

        private void UpdateInstance(MeasuringToolInstance instance)
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

            var sessionEntities = EntityGameObject.ActiveEntities.Where(e => e.Entity.sessionId == instance.Session.sessionId);
            var entity0 = sessionEntities.FirstOrDefault(e => e.Entity.name == Entities[0]);
            var entity1 = sessionEntities.FirstOrDefault(e => e.Entity.name == Entities[1]);

            // Render line
            if (entity0 && entity1)
            {
                instance.positions[0] = entity0.transform.position;
                instance.positions[1] = entity1.transform.position;
                instance.fastLineRenderer.SetPoints(instance.positions, instance.colors);
                // fastLineRenderer.ShowLineSegment(0, 1);

                // Calculate distance
                instance.distance = Vector3.Distance(entity0.transform.position, entity1.transform.position);

                // Update tool badge
                instance.toolValueBadge.Position = Vector3.Lerp(entity0.transform.position, entity1.transform.position, 0.5f);
                instance.toolValueBadge.Value = instance.GetResult();
            }
        }

        public override void RemoveEvent(string eventName)
        {
            
        }

        public override void AddEvent(string eventName)
        {
            
        }

        public override bool AddEvent(EventGameObject eventGameObject)
        {
            return false;
        }
    }
}
