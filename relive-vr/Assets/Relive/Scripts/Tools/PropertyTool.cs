using UnityEngine;
using Relive.Data;
using Relive.Tools.Parameter;
using System.Linq;

namespace Relive.Tools
{
    public class PropertyTool : Tool
    {
        private class PropertyToolInstance : ToolInstance
        {
            public PropertyToolPanel PropertyPanel;

            public override string GetResult()
            {
                return "";
            }
        }

        [Header("Custom Properties")]
        public PropertyToolPanel PropertyToolPanelPrefab;

        public override void AddEntity(string entityName)
        {
            if (Entities.Count < MaxEntities && !Entities.Contains(entityName))
                Entities.Add(entityName);

            foreach (var p in Parameters.Values)
            {
                if (p is BoolToolParameter bp)
                {
                    bp.OnValueChanged -= OnPropertySelected;
                    bp.OnValueChanged += OnPropertySelected;
                    OnPropertySelected(bp);
                }
            }
        }

        public override void RemoveEntity(string entityName)
        {
            if (Entities.Count == 1)
            {
                foreach (PropertyToolInstance instance in Instances)
                    DetachInstance(instance);
            }

            Entities.Remove(entityName);
            Parameters.Clear();
        }

        public override bool AddEntity(EntityGameObject entityGameObject)
        {
            if (Entities.Count < MaxEntities)
            {
                Entities.Add(entityGameObject.name);

                foreach (var attribute in entityGameObject.Entity.attributes)
                {
                    BoolToolParameter boolToolParameter = new BoolToolParameter();
                    boolToolParameter.OnValueChanged += OnPropertySelected;
                    boolToolParameter.Name = attribute;
                    Parameters.Add(attribute, boolToolParameter);
                }

                return true;
            }
            return false;
        }

        private void Update()
        {
            foreach (PropertyToolInstance instance in Instances)
            {
                var entity = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == Entities[0] && e.Entity.sessionId == instance.Session.sessionId);
                if (entity != null)
                {
                    if (entity.propertyToolPanel == null)
                    {
                        instance.PropertyPanel = Instantiate(PropertyToolPanelPrefab);
                        entity.propertyToolPanel = instance.PropertyPanel;
                        instance.PropertyPanel.entityGameObject = entity;

                        foreach (var parameter in Parameters.Values)
                        {
                            if (parameter is BoolToolParameter bp && bp.Value)
                                instance.PropertyPanel?.AddPropertyToolPanelItem(bp.Name);
                        }
                    }
                }
                else if (instance.PropertyPanel != null)
                {
                    Destroy(instance.PropertyPanel.gameObject);
                    instance.PropertyPanel = null;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (PropertyToolInstance instance in Instances)
                DetachInstance(instance);
        }

        private void DetachInstance(PropertyToolInstance instance)
        {
            var entity = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == Entities[0] && e.Entity.sessionId == instance.Session.sessionId);
            if (entity)
                entity.propertyToolPanel = null;
            if (instance.PropertyPanel != null)
                Destroy(instance.PropertyPanel.gameObject);
        }


        private void OnPropertySelected(BoolToolParameter selection)
        {
            foreach (PropertyToolInstance instance in Instances)
            {
                if (selection.Value)
                    instance.PropertyPanel?.AddPropertyToolPanelItem(selection.Name);
                else
                    instance.PropertyPanel?.DestroyPropertyToolPanelItem(selection.Name);
            }
        }

        public override void AddInstance(Session session, Color color)
        {
            if (Instances.Any(i => i.Session == session))
                return;

            var instance = new PropertyToolInstance
            {
                Session = session,
                Color = color
            };

            Instances.Add(instance);
            InvokeInstanceAdded(instance);
        }

        public override void RemoveInstance(Session session)
        {
            var toolInstance = Instances.FirstOrDefault(t => t.Session == session) as PropertyToolInstance;
            if (toolInstance != null)
            {
                DetachInstance(toolInstance);
                Instances.Remove(toolInstance);
                InvokeInstanceRemoved(toolInstance);
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