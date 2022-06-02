using System.Linq;
using UnityEngine;
using Relive.Data;
using Relive.Visualizations;

namespace Relive.Tools
{
    public class AngleTool : Tool, IVisualizable
    {
        private class AngleToolInstance : ToolInstance
        {
            public float angle = 0.0f;
            public string fFormationName = "";
            
            // New line rendering
            public FastLineRenderer fastLineRenderer;
            public Vector3[] positions = new Vector3[5];
            public Color[] colors = new Color[5];

            // Old line rendering
            public Material material;

            public override string GetResult()
            {
                return angle.ToString("f2") + "°";
            }
        }

        [Header("Custom Properties")]
        public FastLineRenderer LineRendererPrefab;

        private bool oldLineRendering = true;
        private bool newLineRendering = false;

        // old line rendering
        private GameObject[] cameras;

        void OnEnable()
        {
            // Old line rendering
            if (oldLineRendering)
            {
                cameras = GameObject.FindGameObjectsWithTag("MainCamera");
                foreach (GameObject cam in cameras)
                    cam.GetComponent<Visualizer>()?.registerVisualizable(this);
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

        private void OnColorChanged(ToolInstance instance)
        {
            var ati = instance as AngleToolInstance;
            ati.colors[0] = ati.Color;
            ati.colors[1] = ati.Color;
            ati.colors[2] = ati.Color;
            ati.colors[3] = ati.Color;
            ati.colors[4] = ati.Color;

            // Old line rendering
            if (oldLineRendering)
                ati.material.color = ati.Color;
        }

        public void Update()
        {
            //if (newLineRendering && RenderVisualization && Entities.Count == 2 && Entities[0] != null && Entities[1] != null)
            //{
            //    Vector3 p1 = Entities[0].transform.rotation * Vector3.forward;
            //    Vector3 p2 = Entities[1].transform.rotation * Vector3.forward;

            //    // Calculate angle
            //    angle = Vector3.Angle(new Vector3(p1.x, 0, p1.z), new Vector3(p2.x, 0, p2.z));

            //    // Get f formation type
            //    if (angle <= 60)
            //    {
            //        fFormationName = "Side-to-side";
            //    }
            //    else if (angle > 60 && angle <= 120)
            //    {
            //        fFormationName = "Corner-to-corner";
            //    }
            //    else
            //    {
            //        fFormationName = "Face-to-Face";
            //    }

            //    // Get intersection
            //    bool found;
            //    Vector2 intersection = GetIntersectionPointCoordinates(
            //        new Vector2(Entities[0].transform.position.x, Entities[0].transform.position.z),
            //        new Vector2(Entities[0].transform.position.x, Entities[0].transform.position.z) + new Vector2(Entities[0].transform.forward.x, Entities[0].transform.forward.z),
            //        new Vector2(Entities[1].transform.position.x, Entities[1].transform.position.z),
            //        new Vector2(Entities[1].transform.position.x, Entities[1].transform.position.z) + new Vector2(Entities[1].transform.forward.x, Entities[1].transform.forward.z),
            //        out found
            //    );

            //    // Render lines
            //    if (found)
            //    {
            //        positions[0] = Entities[0].transform.position; // Position of first entity
            //        positions[1] = new Vector3(Entities[0].transform.position.x, 0.05f, Entities[0].transform.position.z); // Floor position of first entity
            //        positions[2] = new Vector3(intersection.x, 0.05f, intersection.y); // Floor position of intersection
            //        positions[3] = new Vector3(Entities[1].transform.position.x, 0.05f, Entities[1].transform.position.z); // Floor position of second entity
            //        positions[4] = Entities[1].transform.position; // Position of second entity
            //        fastLineRenderer.SetPoints(positions, colors);
            //        fastLineRenderer.ShowLineSegment(0, 4);
            //    }
            //}
        }

        public Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
        {
            float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

            if (tmp == 0)
            {
                // No solution!
                found = false;
                return Vector2.zero;
            }

            float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

            found = true;

            return new Vector2(
                B1.x + (B2.x - B1.x) * mu,
                B1.y + (B2.y - B1.y) * mu
            );
        }

        // Old line rendering
        public void Visualize()
        {
            if (oldLineRendering && RenderVisualization && Entities.Count == 2 && Entities[0] != null && Entities[1] != null)
            {
                foreach (AngleToolInstance instance in Instances)
                    DrawInstance(instance);
            }
        }

        private void DrawInstance(AngleToolInstance instance)
        {
            var sessionEntities = EntityGameObject.ActiveEntities.Where(e => e.Entity.sessionId == instance.Session.sessionId);
            var entity0 = sessionEntities.FirstOrDefault(e => e.Entity.name == Entities[0]);
            var entity1 = sessionEntities.FirstOrDefault(e => e.Entity.name == Entities[1]);

            if (!entity0 || !entity1)
                return;

            Vector3 p1 = entity0.transform.rotation * Vector3.forward;
            Vector3 p2 = entity1.transform.rotation * Vector3.forward;

            // Calculate angle
            instance.angle = Vector3.Angle(new Vector3(p1.x, 0, p1.z), new Vector3(p2.x, 0, p2.z));

            // Get f formation type
            if (instance.angle <= 60)
                instance.fFormationName = "Side-to-side";
            else if (instance.angle > 60 && instance.angle <= 120)
                instance.fFormationName = "Corner-to-corner";
            else
                instance.fFormationName = "Face-to-Face";

            // Get intersection
            bool found;
            var et0 = entity0.transform;
            var et1 = entity1.transform;
            Vector2 intersection = GetIntersectionPointCoordinates(
                new Vector2(et0.position.x, et0.position.z),
                new Vector2(et0.position.x, et0.position.z) + new Vector2(et0.forward.x, et0.forward.z),
                new Vector2(et1.position.x, et1.position.z),
                new Vector2(et1.position.x, et1.position.z) + new Vector2(et1.forward.x, et1.forward.z),
                out found
            );

            // Render lines
            if (found)
            {
                GL.Begin(GL.LINES);
                drawGlLine(instance, et0.position, new Vector3(et0.position.x, 0.05f, et0.position.z));
                drawGlLine(instance, et1.position, new Vector3(et1.position.x, 0.05f, et1.position.z));
                drawGlLine(
                    instance,
                    new Vector3(et0.position.x, 0.05f, et0.position.z),
                    new Vector3(intersection.x, 0.05f, intersection.y)
                );
                drawGlLine(
                    instance,
                    new Vector3(et1.position.x, 0.05f, et1.position.z),
                    new Vector3(intersection.x, 0.05f, intersection.y)
                );
                GL.End();
            }
        }

        private void drawGlLine(AngleToolInstance instance, Vector3 start, Vector3 end)
        {
            instance.material.SetPass(0);
            GL.Color(instance.material.color);
            GL.Vertex(start);
            GL.Vertex(end);
        }

        private void OnDestroy()
        {
            foreach (GameObject cam in cameras)
            {
                if (cam && cam.GetComponent<Visualizer>() != null)
                    cam.GetComponent<Visualizer>().deregisterVisualizable(this);
            }
        }

        public override void AddInstance(Session session, Color color)
        {
            if (Instances.Any(i => i.Session == session))
                return;

            var instance = new AngleToolInstance
            {
                Session = session,
                Color = color
            };

            if (newLineRendering)
                instance.fastLineRenderer = Instantiate(LineRendererPrefab);
            if (oldLineRendering)
            {
                instance.material = new Material(Shader.Find("Unlit/Color"));
                instance.material.color = color;
            }

            instance.OnColorUpdate += OnColorChanged;
            OnColorChanged(instance);
            Instances.Add(instance);

            InvokeInstanceAdded(instance);
        }

        public override void RemoveInstance(Session session)
        {
            var toolInstance = Instances.FirstOrDefault(t => t.Session == session) as AngleToolInstance;
            if (toolInstance != null)
            {
                toolInstance.OnColorUpdate -= OnColorChanged;
                Instances.Remove(toolInstance);

                if (toolInstance.fastLineRenderer)
                    Destroy(toolInstance.fastLineRenderer.gameObject);

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
