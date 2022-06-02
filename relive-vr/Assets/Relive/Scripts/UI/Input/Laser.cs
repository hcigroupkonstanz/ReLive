using Relive.Data;
using UnityEngine;
using UnityEngine.XR;
// using Valve.VR;

namespace Relive.UI.Input
{
    public class Laser : MonoBehaviour
    {
        public static Laser Instance;

        public delegate void GameObjectSelectedHandler(GameObject gameObject);
        public event GameObjectSelectedHandler OnGameObjectSelected;

        public bool ShowLaserRay = true;
        public bool EnableHover = true;
        // public Camera ScreenCamera;
        public GameObject LaserPrefab;
        public Vector3 PositionOffset;
        public float XRotation;
        public float DefaultLength = 2f;
        public Color ValidColor;
        public Color InvalidColor;

        [HideInInspector]
        public RaycastHit RaycastHit;

        [HideInInspector]
        public bool Hitting = false;

        [HideInInspector]
        public Vector3 HitPoint;

        [HideInInspector]
        public Vector3 HitNormal;

        [HideInInspector]
        public Vector3 LaserEndPoint;

        [HideInInspector]
        public Vector3 LaserStartPosition;

        [HideInInspector]
        public Vector3 LaserForwardDirection;

        // [HideInInspector]
        // public bool Valid = false;

        private GameObject laser;
        // private HoverInfo hoverInfo;
        private bool lastTriggerButtonPressed = false;
        private string lastHoverEntityName;
        private string lastHoverEventName;
        private EventGameObject lastHoverEventGameObject;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (ShowLaserRay)
            {
                laser = Instantiate(LaserPrefab);
            }
        }

        void Update()
        {
            // Calculate laser with offset
            LaserStartPosition = transform.TransformPoint(PositionOffset);
            LaserForwardDirection = Quaternion.AngleAxis(XRotation, transform.right) * transform.forward;

            // Raycast
            if (Physics.Raycast(LaserStartPosition, LaserForwardDirection, out RaycastHit, 100))
            {
                HitPoint = RaycastHit.point;
                HitNormal = RaycastHit.normal;
                Hitting = true;
                // Valid = true;
                if (ShowLaserRay)
                {
                    ShowLaser();
                }
                if (EnableHover)
                {
                    ShowHover();
                }

            }
            else
            {
                HitPoint = LaserStartPosition + LaserForwardDirection;
                HitNormal = new Vector3(0f, 1f, 0f);
                Hitting = false;
                // Valid = false;
                if (ShowLaserRay)
                {
                    ShowLaser();
                }
                if (EnableHover)
                {
                    ShowHover();
                }
            }

            // Handle right trigger pressed
            bool triggerButtonPressed = false;
            InputManagerVR.Instance.RightInputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonPressed);
            if (triggerButtonPressed && !lastTriggerButtonPressed && Hitting) OnGameObjectSelected?.Invoke(RaycastHit.transform.gameObject);
            lastTriggerButtonPressed = triggerButtonPressed;
        }

        private void ShowLaser()
        {
            laser.SetActive(true);
            if (Hitting)
            {
                laser.GetComponent<Renderer>().material.color = ValidColor;
            }
            else
            {
                laser.GetComponent<Renderer>().material.color = InvalidColor;
            }
            LaserEndPoint = HitPoint;
            if (!Hitting) LaserEndPoint = LaserStartPosition + (LaserForwardDirection * DefaultLength);
            laser.transform.position = Vector3.Lerp(LaserStartPosition, LaserEndPoint, .5f);
            laser.transform.LookAt(LaserEndPoint);
            float distance = (LaserEndPoint - LaserStartPosition).magnitude;
            laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, distance);
        }

        private void ShowHover()
        {
            if (Hitting)
            {
                EntityGameObject entityGameObject;
                EventGameObject eventGameObject;
                if (entityGameObject = EntityGameObject.IsEntityGameObject(RaycastHit.transform.gameObject))
                {
                    string entityName = entityGameObject.Entity.name;
                    foreach (EntityGameObject activeEntityGameObject in EntityGameObject.ActiveEntities)
                    {
                        if (activeEntityGameObject.Entity.name == entityName)
                        {
                            activeEntityGameObject.ShowHoverInfo();
                        }
                    }

                    // If event was hovered before hide hover
                    if (lastHoverEventName != null)
                    {
                        foreach (EventGameObject activeEventGameObject in EventGameObject.ActiveEvents)
                        {
                            if (activeEventGameObject.Event.name == lastHoverEventName)
                            {
                                activeEventGameObject.HideHoverInfo();
                            }
                        }
                        lastHoverEntityName = null;
                    }
                    if (lastHoverEventGameObject)
                    {
                        lastHoverEventGameObject.HideHoverInfo();
                        lastHoverEventGameObject = null;
                    }

                    // If an other entity was hovered before hide hover
                    if (lastHoverEntityName != null && lastHoverEntityName != entityName)
                    {
                        foreach (EntityGameObject activeEntityGameObject in EntityGameObject.ActiveEntities)
                        {
                            if (activeEntityGameObject.Entity.name == lastHoverEntityName)
                            {
                                activeEntityGameObject.HideHoverInfo();
                            }
                        }
                    }
                    lastHoverEntityName = entityName;
                }
                else if (eventGameObject = EventGameObject.IsEventGameObject(RaycastHit.transform.gameObject))
                {
                    string eventName = eventGameObject.Event.name;
                    if (eventName != null)
                    {
                        foreach (EventGameObject activeEventGameObject in EventGameObject.ActiveEvents)
                        {
                            if (activeEventGameObject.Event.name == eventName)
                            {
                                activeEventGameObject.ShowHoverInfo();
                            }
                        }
                    }
                    else
                    {
                        eventGameObject.ShowHoverInfo();
                    }

                    // If entity was hovered before hide hover
                    if (lastHoverEntityName != null)
                    {
                        foreach (EntityGameObject activeEntityGameObject in EntityGameObject.ActiveEntities)
                        {
                            if (activeEntityGameObject.Entity.name == lastHoverEntityName)
                            {
                                activeEntityGameObject.HideHoverInfo();
                            }
                        }
                        lastHoverEntityName = null;
                    }

                    // If an other event was hovered before hide hover
                    // TODO
                    if (lastHoverEventName != null && lastHoverEventName != eventName)
                    {
                        foreach (EventGameObject activeEventGameObject in EventGameObject.ActiveEvents)
                        {
                            if (activeEventGameObject.Event.name == lastHoverEventName)
                            {
                                activeEventGameObject.HideHoverInfo();
                            }
                        }
                        lastHoverEventName = null;
                    }
                    if (lastHoverEventGameObject && lastHoverEventGameObject != eventGameObject)
                    {
                        lastHoverEventGameObject.HideHoverInfo();
                        lastHoverEventGameObject = null;
                    }

                    if (eventName != null)
                    {
                        lastHoverEventName = eventName;
                    }
                    else
                    {
                        lastHoverEventGameObject = eventGameObject;
                    }
                }
                return;
            }
            if (lastHoverEntityName != null)
            {
                foreach (EntityGameObject activeEntityGameObject in EntityGameObject.ActiveEntities)
                {
                    if (activeEntityGameObject.Entity.name == lastHoverEntityName)
                    {
                        activeEntityGameObject.HideHoverInfo();
                    }
                }
                lastHoverEntityName = null;
            }
            if (lastHoverEventName != null)
            {
                foreach (EventGameObject activeEventGameObject in EventGameObject.ActiveEvents)
                {
                    if (activeEventGameObject.Event.name == lastHoverEventName)
                    {
                        activeEventGameObject.HideHoverInfo();
                    }
                }
                lastHoverEventName = null;
            }
            if (lastHoverEventGameObject)
            {
                lastHoverEventGameObject.HideHoverInfo();
                lastHoverEventGameObject = null;
            }
        }
    }
}
