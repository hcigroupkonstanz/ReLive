using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
// using Valve.VR;

namespace Relive.UI
{
    public class ControllerPanelVR : MonoBehaviour
    {
        public XRController Controller;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        void Update()
        {
            if (Controller)
            {
                transform.position = Controller.transform.position + (Controller.transform.right * positionOffset.x) + (Controller.transform.up * positionOffset.y) + (Controller.transform.forward * positionOffset.z);
                transform.rotation = Controller.transform.rotation * Quaternion.Euler(rotationOffset);
            }
        }
    }
}
