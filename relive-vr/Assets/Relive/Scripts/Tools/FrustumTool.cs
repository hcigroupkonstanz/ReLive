using UnityEngine;
using Relive.Data;
using Relive.Visualizations;
using System.Linq;

namespace Relive.Tools
{
    public class FrustumTool : Tool, IVisualizable //IMultiSelection
    {
        [Header("Custom Properties")]
        // Frustum calculation (Default = Vive Pro)
        public float HorizontalFOV = 110f;
        public int HorizontalResolution = 2880;
        public int VerticalResolution = 1600;
        public float Zoom = 1f;
        public float FrustumDistance = 20f;

        private class FrustumToolInstance : ToolInstance
        {
            public float HorizontalFOV = 110f;
            public Material Material;

            public override string GetResult()
            {
                return HorizontalFOV + "°";
            }
        }

        private Vector3[] frustumCorners = new Vector3[4];
        private Vector3[] frustumCornersJonathanIPadPro =
        {
            new Vector3(-8.8f, -6.6f, 20.0f),
            new Vector3(-8.8f, 6.6f, 20.0f),
            new Vector3(8.8f, 6.6f, 20.0f),
            new Vector3(8.8f, -6.6f, 20.0f)
        };
        private Vector3 bottomLeftCorner;
        private Vector3 topLeftCorner;
        private Vector3 bottomRightCorner;
        private Vector3 topRightCorner;


        // Line rendering
        private Material material;
        private GameObject[] cameras;

        private void OnEnable()
        {
            // Line Rendering
            cameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (GameObject cam in cameras)
            {
                if (cam.GetComponent<Visualizer>()) cam.GetComponent<Visualizer>().registerVisualizable(this);
            }
            material = new Material(Shader.Find("Unlit/Color"));
        }

        public override void AddEntity(string entityName)
        {
            if (Entities.Count < MaxEntities && !Entities.Contains(entityName))
            {
                DetectFOV(entityName);
                CalculateCorners();
                Entities.Add(name);

                foreach (FrustumToolInstance instance in Instances)
                    instance.HorizontalFOV = HorizontalFOV;
            }
        }

        public override void RemoveEntity(string entityName)
        {
            Entities.Remove(entityName);

            if (Entities.Count == 0)
            {
                foreach (FrustumToolInstance instance in Instances)
                    instance.HorizontalFOV = 0;
            }
        }

        public override bool AddEntity(EntityGameObject entityGameObject)
        {
            if (Entities.Count < MaxEntities)
            {
                bool nameSuccess = DetectFOV(entityGameObject.Entity.name);
                if (!nameSuccess && entityGameObject.Entity.filePaths != null)
                    DetectFOV(entityGameObject.Entity.filePaths);

                CalculateCorners();

                Entities.Add(entityGameObject.name);

                foreach (FrustumToolInstance instance in Instances)
                    instance.HorizontalFOV = HorizontalFOV;

                return true;
            }
            return false;
        }

        public void Visualize()
        {
            if (RenderVisualization && Entities.Count == 1)
            {
                foreach (FrustumToolInstance instance in Instances)
                {
                    var entity = EntityGameObject.ActiveEntities.FirstOrDefault(e => e.Entity.name == Entities[0] && e.Entity.sessionId == instance.Session.sessionId);
                    if (entity == null)
                        continue;

                    var tf = entity.transform;

                    bottomLeftCorner = tf.forward + (19 * tf.forward) + (frustumCorners[0].x * tf.right) + (frustumCorners[0].y * tf.up);
                    topLeftCorner = tf.forward + (19 * tf.forward) + (frustumCorners[1].x * tf.right) + (frustumCorners[1].y * tf.up);
                    bottomRightCorner = tf.forward + (19 * tf.forward) + (frustumCorners[2].x * tf.right) + (frustumCorners[2].y * tf.up);
                    topRightCorner = tf.forward + (19 * tf.forward) + (frustumCorners[3].x * tf.right) + (frustumCorners[3].y * tf.up);

                    GL.Begin(GL.LINES);
                    drawGlLine(instance, tf.position, bottomLeftCorner);
                    drawGlLine(instance, tf.position, topLeftCorner);
                    drawGlLine(instance, tf.position, bottomRightCorner);
                    drawGlLine(instance, tf.position, topRightCorner);
                    GL.End();
                }
            }
        }


        private void drawGlLine(FrustumToolInstance instance, Vector3 start, Vector3 end)
        {
            instance.Material.color = instance.Color;
            instance.Material.SetPass(0);
            GL.Color(instance.Color);
            GL.Vertex(start);
            GL.Vertex(end);
        }

        void OnDestroy()
        {
            foreach (GameObject cam in cameras)
            {
                if (cam.GetComponent<Visualizer>()) cam.GetComponent<Visualizer>().deregisterVisualizable(this);
            }
        }

        // EXPERIMENTAL FOV CALCULATION
        private void CalculateCorners()
        {
            float horizontalFOVCorner = FOVToCorner(HorizontalFOV, FrustumDistance) / Zoom;
            float resolutionRatio = (float)VerticalResolution / (float)HorizontalResolution;
            float verticalFOVCorner = resolutionRatio * horizontalFOVCorner;

            frustumCorners[0] = new Vector3(horizontalFOVCorner * -1f, verticalFOVCorner * -1f, FrustumDistance);
            frustumCorners[1] = new Vector3(horizontalFOVCorner * -1f, verticalFOVCorner, FrustumDistance);
            frustumCorners[2] = new Vector3(horizontalFOVCorner, verticalFOVCorner, FrustumDistance);
            frustumCorners[3] = new Vector3(horizontalFOVCorner, verticalFOVCorner * -1f, FrustumDistance);
        }
        private float FOVToCorner(float angle, float distance)
        {
            float halfAngle = angle / 2;
            float halfAngleRadians = halfAngle * Mathf.Deg2Rad;
            return Mathf.Tan(halfAngleRadians) * distance;
        }

        // WARNING: VERY BASIC DEVICE DETECTION
        private bool DetectFOV(string deviceName)
        {
            string lowerDeviceName = deviceName.ToLower();
            if (lowerDeviceName.Contains("ipad"))
            {
                HorizontalFOV = 57.716f;
                HorizontalResolution = 2048;
                VerticalResolution = 1536;
                Zoom = 1.25f;
                return true;
            }
            else if (lowerDeviceName.Contains("hololens"))
            {
                HorizontalFOV = 34f;
                HorizontalResolution = 1280;
                VerticalResolution = 720;
                Zoom = 1f;
                return true;
            }
            return false;
        }

        public override void AddInstance(Session session, Color color)
        {
            if (Instances.Any(i => i.Session == session))
                return;

            var instance = new FrustumToolInstance
            {
                Session = session,
                Color = color,
                HorizontalFOV = HorizontalFOV,
                Material = Instantiate(material)
            };

            Instances.Add(instance);
            InvokeInstanceAdded(instance);
        }

        public override void RemoveInstance(Session session)
        {
            var toolInstance = Instances.FirstOrDefault(t => t.Session == session) as FrustumToolInstance;
            if (toolInstance != null)
            {
                Destroy(toolInstance.Material);
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