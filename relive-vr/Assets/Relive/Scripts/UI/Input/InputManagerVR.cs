using System.Collections.Generic;
using HCIKonstanz.Colibri;
using Relive.Playback;
using Relive.Sync;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Relive.UI.Input
{
    public class InputManagerVR : MonoBehaviour
    {
        public static InputManagerVR Instance;
        public float WindFactor = 0.5f;
        public float ScrollFactor = 0.5f;
        public InputDevice RightInputDevice;
        public XRController RightXRController;
        public InputDevice LeftInputDevice;
        public XRController LeftXRController;

        [Header("Controller Model Prefabs")]
        public GameObject DefaultControllerPrefab;
        public GameObject OculusQuestLeftControllerPrefab;
        public GameObject OculusQuestRightControllerPrefab;
        public GameObject OculusTouchLeftControllerPrefab;
        public GameObject OculusTouchRightControllerPrefab;
        public GameObject ValveIndexLeftControllerPrefab;
        public GameObject ValveIndexRightControllerPrefab;
        public GameObject ViveLeftControllerPrefab;
        public GameObject ViveRightControllerPrefab;


        // private InputDevice device;
        private bool knobTouched = false;
        private bool triggerTouched = false;
        private bool primaryPressed = false;
        private bool wasPlaybackPaused = false;

        void Awake()
        {
            Instance = this;
        }

        void OnEnable()
        {
            List<InputDevice> allDevices = new List<InputDevice>();
            InputDevices.GetDevices(allDevices);
            foreach (InputDevice device in allDevices)
                InputDevices_deviceConnected(device);

            InputDevices.deviceConnected += InputDevices_deviceConnected;
            InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
        }

        void OnDisable()
        {
            InputDevices.deviceConnected -= InputDevices_deviceConnected;
            InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        }

        private void InputDevices_deviceConnected(InputDevice device)
        {
            if ((device.characteristics & InputDeviceCharacteristics.Right) == InputDeviceCharacteristics.Right)
            {
                RightInputDevice = device;
            }

            if ((device.characteristics & InputDeviceCharacteristics.Left) == InputDeviceCharacteristics.Left)
            {
                LeftInputDevice = device;
            }
        }

        private void InputDevices_deviceDisconnected(InputDevice device)
        {

        }

        void Update()
        {
            // HINT: In future this will be moved to the playbackmanager ???

            // Check if primary axis is touched
            bool primary2DAxisTouched = false;
            RightInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out primary2DAxisTouched);
            OnTouch(primary2DAxisTouched);

            // Get current position of primary axis
            Vector2 primary2DAxisPosition;
            RightInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxisPosition);
            OnUpdate(primary2DAxisPosition);

        }

        private void OnTouch(bool newState)
        {
            var notebook = NotebookManager.Instance.Notebook;
            if (!knobTouched && newState)
            {
                // PlaybackManager.Instance.Pause();
                wasPlaybackPaused = notebook.IsPaused;
                notebook.IsPaused = true;
                notebook.UpdateLocal();
            }
            else if (knobTouched && !newState)
            {
                // PlaybackManager.Instance.Play();
                notebook.IsPaused = wasPlaybackPaused;
                notebook.UpdateLocal();
            }
            knobTouched = newState;
        }

        private void OnUpdate(Vector2 axis)
        {
            if (knobTouched)
            {
                // Scroll with right joystick
                // if (Laser.Instance.Hitting && Laser.Instance.RaycastHit.transform.gameObject.GetComponent<ScrollRect>())
                // {
                //     // ScrollRect scrollRect = RightLaser.RaycastHit.transform.gameObject.GetComponent<ScrollRect>();
                //     // scrollRect.verticalNormalizedPosition += Mathf.Pow(axis.y, 3) * ScrollFactor;
                // }
                // // Wind with right joystick
                // else
                // {
                float windSeconds = Mathf.Pow(axis.x, 3) * WindFactor;
                PlaybackManager.Instance.Wind(windSeconds);
                // }
            }
        }
    }
}
