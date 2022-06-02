using System;
using Relive.Data;
using Relive.Playback.Data;
using UnityEngine;
using Relive.Tools.Parameter;
using Relive.Visualizations;
using System.Collections.Generic;
using System.Linq;

namespace Relive.Tools
{
    public class TrailTool : Tool
    {
        private class TrailToolInstance : ToolInstance
        {
            public FastLineRenderer fastLineRenderer;
            public List<Vector3> positions;
            public List<Color> colors;
            public List<Tuple<int, Vector3>> directionIndicatorPositions;
            public ToolInstance toolInstance;

            public Dictionary<int, GameObject> directionIndicators = new Dictionary<int, GameObject>();

            public float distanceMoved = 0.0f;

            public override string GetResult()
            {
                return distanceMoved.ToString("f2") + "m";
            }
        }

        [Header("Custom Properties")]
        public GameObject DirectionIndicatorPrefab;
        public float DistanceBetweenIndicators = 0.2f;
        public FastLineRenderer RendererTemplate;

        // Parameter
        private BoolToolParameter mapSpeedToColorParameter;
        private BoolToolParameter showFullTrailParameter;
        private NumberToolParameter trailLengthParameter;

        void OnEnable()
        {
            // Generate parameters
            mapSpeedToColorParameter = new BoolToolParameter();
            mapSpeedToColorParameter.Name = "Map Speed to Color";
            mapSpeedToColorParameter.Value = false;
            mapSpeedToColorParameter.OnValueChanged += OnMapSpeedToColorChanged;
            Parameters.Add("speed", mapSpeedToColorParameter);

            showFullTrailParameter = new BoolToolParameter();
            showFullTrailParameter.Name = "Show Full Trail";
            showFullTrailParameter.Value = false;
            Parameters.Add("fulltrail", showFullTrailParameter);

            trailLengthParameter = new NumberToolParameter();
            trailLengthParameter.Name = "Trail Length (s)";
            trailLengthParameter.Value = 600f;
            trailLengthParameter.MinValue = 10f;
            trailLengthParameter.StepSize = 10f;
            trailLengthParameter.Unit = "s";
            Parameters.Add("traillength", trailLengthParameter);
        }

        public override void AddEntity(string entityName)
        {
            if (Entities.Count < MaxEntities && !Entities.Contains(entityName))
            {
                Entities.Add(entityName);
                foreach (TrailToolInstance instance in Instances)
                    CalculateTrail(instance);
            }
        }

        public override void RemoveEntity(string entityName)
        {
            Entities.Remove(entityName);

            if (Entities.Count == 0)
            {
                foreach (TrailToolInstance instance in Instances)
                    DisableInstance(instance);
            }
        }


        public override bool AddEntity(EntityGameObject entityGameObject)
        {
            if (Entities.Count < MaxEntities)
            {
                Entities.Add(entityGameObject.name);
                foreach (TrailToolInstance instance in Instances)
                    CalculateTrail(instance);
                return true;
            }
            return false;
        }

        public override void AddInstance(Session session, Color color)
        {
            if (Instances.Any(i => i.Session == session))
                return;

            var instance = new TrailToolInstance
            {
                Session = session,
                Color = color,
                fastLineRenderer = Instantiate(RendererTemplate, transform)
            };

            CalculateTrail(instance);
            instance.OnColorUpdate += OnColorChanged;
            Instances.Add(instance);

            InvokeInstanceAdded(instance);
        }

        public override void RemoveInstance(Session session)
        {
            var toolInstance = Instances.FirstOrDefault(t => t.Session == session) as TrailToolInstance;
            if (toolInstance != null)
            {
                toolInstance.OnColorUpdate -= OnColorChanged;
                Instances.Remove(toolInstance);
                Destroy(toolInstance.fastLineRenderer.gameObject);

                foreach (var go in toolInstance.directionIndicators.Values)
                    Destroy(go);

                InvokeInstanceRemoved(toolInstance);
            }

        }

        private void DisableInstance(TrailToolInstance instance)
        {
            instance.IsActive = false;
            instance.fastLineRenderer.enabled = false;
            foreach (var go in instance.directionIndicators.Values)
                Destroy(go);
            instance.directionIndicators.Clear();
        }

        private void OnDestroy()
        {
            foreach (TrailToolInstance instance in Instances)
            {
                foreach (var go in instance.directionIndicators.Values)
                    Destroy(go);
            }
        }

        public void OnColorChanged(ToolInstance instance)
        {
            var toolInstance = instance as TrailToolInstance;
            CalculateTrail(toolInstance);
            foreach (var kv in toolInstance.directionIndicators)
                kv.Value.GetComponent<Renderer>().material.color = toolInstance.colors[kv.Key];
        }

        public void OnMapSpeedToColorChanged(BoolToolParameter boolToolParameter)
        {
            foreach (TrailToolInstance instance in Instances)
            {
                CalculateTrail(instance);
                foreach (var kv in instance.directionIndicators)
                {
                    kv.Value.GetComponent<Renderer>().material.color = instance.colors[kv.Key];
                }
            }
        }

        private void CalculateTrail(TrailToolInstance instance)
        {
            if (Entities.Count == 0)
                return;

            var entity = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == Entities[0] && e.Entity.sessionId == instance.Session.sessionId);
            if (entity == null)
                return;

            instance.colors = new List<Color>();

            if (!instance.IsActive)
            {
                instance.positions = new List<Vector3>();
                instance.directionIndicatorPositions = new List<Tuple<int, Vector3>>();
            }

            Vector3 lastIndicatorPosition = Vector3.zero;

            for (int i = 0; i < entity.States.Count; i++)
            {
                if (entity.States[i].position != null)
                {
                    // Calculate trail colors
                    var color = instance.Color;
                    // Map color to speed 
                    if (mapSpeedToColorParameter.Value)
                    {
                        color = Color.Lerp(Color.white, instance.Color, Mathf.Log(1 + entity.States[i].speed) / Mathf.Log(1 + entity.Entity.maxSpeed));
                    }
                    instance.colors.Add(color);

                    // Calculate trail positions
                    if (!instance.IsActive)
                    {
                        Transform sceneObjectsTransform = DataPlayerObjects.Instance.gameObject.transform;
                        Vector3 position = sceneObjectsTransform.TransformPoint(entity.States[i].position.GetVector3());
                        instance.positions.Add(position);

                        // Calculate direction indicators
                        if (instance.positions.Count == 1)
                        {
                            Tuple<int, Vector3> directionIndicatorPosition = new Tuple<int, Vector3>(instance.positions.Count - 1, position);
                            instance.directionIndicatorPositions.Add(directionIndicatorPosition);
                            lastIndicatorPosition = position;
                        }
                        else
                        {
                            if (Vector3.Distance(position, lastIndicatorPosition) > DistanceBetweenIndicators)
                            {
                                Tuple<int, Vector3> directionIndicatorPosition = new Tuple<int, Vector3>(instance.positions.Count - 1, position);
                                instance.directionIndicatorPositions.Add(directionIndicatorPosition);
                                lastIndicatorPosition = position;
                            }
                        }
                    }
                }
            }
            instance.fastLineRenderer.SetPoints(instance.positions.ToArray(), instance.colors.ToArray());
            instance.IsActive = true;
        }

        public void Update()
        {
            if (RenderVisualization && Entities.Count == 1 && Entities[0] != null)
            {
                foreach (TrailToolInstance instance in Instances)
                    UpdateInstance(instance);
            }
            else
            {
                foreach (TrailToolInstance instance in Instances)
                    DisableInstance(instance);
            }
        }

        private void UpdateInstance(TrailToolInstance instance)
        {
            if (instance.IsActive && !instance.Session.IsActive)
            {
                // disable instance
                DisableInstance(instance);
            }
            else if (!instance.IsActive && instance.Session.IsActive)
            {
                // init instance
                instance.fastLineRenderer.enabled = true;
                CalculateTrail(instance);
            }

            if (!instance.IsActive)
                return;


            var entity = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == Entities[0] && e.Entity.sessionId == instance.Session.sessionId);
            if (entity == null)
                return;

            if (!instance.fastLineRenderer.enabled)
            {
                instance.fastLineRenderer.enabled = true;
                instance.fastLineRenderer.SetPoints(instance.positions.ToArray(), instance.colors.ToArray());
            }

            int fromIndex = 0;
            int toIndex = 0;

            // Show full trail
            if (showFullTrailParameter.Value)
            {
                toIndex = entity.States.Count - 1;
            }
            // Show partial trail
            else
            {
                // Create a state with timestamp specified in TrailLengthParameter
                State jumpToTimestampState = new State();
                jumpToTimestampState.timestamp = entity.States[entity.PlaybackStateIndex].timestamp - (long)(trailLengthParameter.Value * 1000);
                fromIndex = entity.States.BinarySearch(jumpToTimestampState, new StateTimestampComparer());
                if (fromIndex < 0)
                {
                    fromIndex = ~fromIndex;
                    // trailStartIndex--;
                }
                toIndex = entity.PlaybackStateIndex;
            }

            // Calculate trail length
            instance.distanceMoved = entity.States[toIndex].distanceMoved - entity.States[fromIndex].distanceMoved;

            // Render trail segment
            instance.fastLineRenderer.ShowLineSegment(fromIndex, toIndex);

            // Render direction indicators
            foreach (Tuple<int, Vector3> directionIndicatorPosition in instance.directionIndicatorPositions)
            {
                if (directionIndicatorPosition.Item1 > toIndex) break;
                if (directionIndicatorPosition.Item1 >= fromIndex)
                {
                    // Do nothing when indicator is already instantiated
                    if (instance.directionIndicators.ContainsKey(directionIndicatorPosition.Item1)) continue;
                    // Rotation calculation is not possible when indicator is last position of full trail
                    if (directionIndicatorPosition.Item1 < instance.positions.Count - 1)
                    {
                        GameObject directionIndicatorGameObject = Instantiate(
                            DirectionIndicatorPrefab,
                            directionIndicatorPosition.Item2,
                            Quaternion.LookRotation(instance.positions[directionIndicatorPosition.Item1 + 1] - instance.positions[directionIndicatorPosition.Item1]));
                            directionIndicatorGameObject.GetComponent<Renderer>().material.color = instance.colors[directionIndicatorPosition.Item1];
                        instance.directionIndicators.Add(directionIndicatorPosition.Item1, directionIndicatorGameObject);
                    }
                }
            }
            // Remove direction indicators
            List<int> removeIndicators = new List<int>();
            foreach (int directionIndicatorIndex in instance.directionIndicators.Keys)
            {
                if (directionIndicatorIndex < fromIndex || directionIndicatorIndex > toIndex)
                {
                    Destroy(instance.directionIndicators[directionIndicatorIndex]);
                    removeIndicators.Add(directionIndicatorIndex);
                }
            }
            foreach (int removeIndicator in removeIndicators)
            {
                instance.directionIndicators.Remove(removeIndicator);
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
