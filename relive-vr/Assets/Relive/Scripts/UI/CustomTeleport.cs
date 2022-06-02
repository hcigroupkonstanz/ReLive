using UnityEngine;
using Relive.UI.Input;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Relive.UI.Input
{


    [RequireComponent(typeof(LineRenderer))]
    public class CustomTeleport : LocomotionProvider
    {
        public GameObject TeleportationArea;
        public GameObject TeleportationDestinationPrefab;
        public float TriggerThreshold = 0.7f;

        [Header("Fading")]
        public float FadeDuration = 0.2f;

        [Header("Line")]
        public float LineWidth = 0.01f;
        public int NumberOfLinePoints = 20;
        public Vector3 PositionOffset;
        public float XRotationOffset;

        private Vector2 lastPrimary2DAxisPosition = Vector2.zero;
        private bool isTeleporting = false;
        private Vector3 teleportPosition = Vector3.zero;
        private bool teleportValid = false;
        private GameObject teleportationDestination;
        private LineRenderer lineRenderer;
        private bool lastTriggerButtonPressed = false;


        void OnEnable()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            // Check if primary axis is touched
            Vector2 primary2DAxisPosition;
            InputManagerVR.Instance.LeftInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxisPosition);
            OnPrimaryAxisUpdate(primary2DAxisPosition);

            // Check if left trigger button is pressed
            bool triggerButtonPressed = false;
            InputManagerVR.Instance.LeftInputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonPressed);
            OnTriggerButtonUpdate(triggerButtonPressed);


            if (isTeleporting)
            {
                // Calculate laser with offset
                Vector3 laserStartPosition = transform.TransformPoint(PositionOffset);
                Vector3 laserForwardDirection = Quaternion.AngleAxis(XRotationOffset, transform.right) * transform.forward;

                // Raycast ...
                Ray ray = new Ray(laserStartPosition, laserForwardDirection);
                RaycastHit hit;

                // ... against teleportation area
                if (TeleportationArea.GetComponent<Collider>().Raycast(ray, out hit, 100f))
                {
                    teleportPosition = hit.point;
                    teleportValid = true;

                    if (TeleportationDestinationPrefab)
                    {
                        if (!teleportationDestination)
                        {
                            teleportationDestination = Instantiate(TeleportationDestinationPrefab);
                        }
                        teleportationDestination.transform.position = hit.point;

                        DrawCurvedLine(hit.point);
                    }
                }
                else
                {
                    teleportValid = false;
                    if (teleportationDestination) Destroy(teleportationDestination);
                    if (lineRenderer.enabled) lineRenderer.enabled = false;
                }

                // Update visualization

            }
        }

        private void OnPrimaryAxisUpdate(Vector2 primary2DAxisPosition)
        {
            if (lastPrimary2DAxisPosition.y <= TriggerThreshold && primary2DAxisPosition.y > TriggerThreshold)
            {
                StartTeleport();
            }
            else if (lastPrimary2DAxisPosition.y > TriggerThreshold && primary2DAxisPosition.y <= TriggerThreshold)
            {
                lastPrimary2DAxisPosition = primary2DAxisPosition;

                EndTeleport();
            }

            lastPrimary2DAxisPosition = primary2DAxisPosition;
        }

        private void OnTriggerButtonUpdate(bool triggerButtonPressed)
        {
            if (triggerButtonPressed && !lastTriggerButtonPressed)
            {
                StartTeleport();
            }
            else if (!triggerButtonPressed && lastTriggerButtonPressed)
            {
                EndTeleport();
            }

            lastTriggerButtonPressed = triggerButtonPressed;
        }

        private void StartTeleport()
        {
            // Teleportation starts
            isTeleporting = true;
        }

        private void EndTeleport()
        {
            // Teleportation ends
            if (teleportValid)
            {
                var xrRig = system.xrRig;
                Vector3 heightAdjustment = xrRig.rig.transform.up * xrRig.cameraInRigSpaceHeight;
                Vector3 cameraDestination = teleportPosition + heightAdjustment;
                xrRig.MoveCameraToWorldLocation(cameraDestination);
            }

            if (teleportationDestination) Destroy(teleportationDestination);
            lineRenderer.enabled = false;
            isTeleporting = false;
        }

        private void DrawCurvedLine(Vector3 destinationPosition)
        {
            if (!lineRenderer.enabled) lineRenderer.enabled = true;

            if (NumberOfLinePoints > 0)
            {
                lineRenderer.positionCount = NumberOfLinePoints;
            }

            lineRenderer.startColor = Laser.Instance.ValidColor;
            lineRenderer.endColor = Laser.Instance.ValidColor; //new Color32(0, 208, 255, 255);
            lineRenderer.startWidth = LineWidth;
            lineRenderer.endWidth = LineWidth;

            // Set points of quadratic Bezier curve
            Vector3 p0 = transform.position;
            Vector3 p2 = destinationPosition;

            // Middle point
            Vector3 p1 = Vector3.Lerp(p0, p2, 0.8f);
            p1.y = (p0.y - p2.y) * 0.8f;

            float t;
            Vector3 position;
            for (int i = 0; i < NumberOfLinePoints; i++)
            {
                t = i / (NumberOfLinePoints - 1.0f);
                position = (1.0f - t) * (1.0f - t) * p0 + 2.0f * (1.0f - t) * t * p1 + t * t * p2;
                lineRenderer.SetPosition(i, position);
            }
        }
    }
}
