using System.Collections;
using System.Collections.Generic;
using Relive.UI.Input;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerModel : MonoBehaviour
{

    // public List<GameObject> ControllerPrefabs;

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
        // print(device.name);
        if (device == GetComponent<XRController>().inputDevice)
        {
            // GameObject prefab = ControllerPrefabs.Find(controller => controller.name == device.name);
            GameObject prefab = GetControllerModel(device.name);

            Instantiate(prefab, transform);
        }
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {

    }

    private GameObject GetControllerModel(string deviceName)
    {
        string deviceNameLower = deviceName.ToLower();
        if (deviceNameLower.Contains("quest"))
        {
            if (deviceNameLower.Contains("left"))
            {
                return InputManagerVR.Instance.OculusQuestLeftControllerPrefab;
            }
            else
            {
                return InputManagerVR.Instance.OculusQuestRightControllerPrefab;
            }
        }
        else if (deviceNameLower.Contains("miramar"))
        {
            if (deviceNameLower.Contains("left"))
            {
                return InputManagerVR.Instance.OculusQuestLeftControllerPrefab;
            }
            else
            {
                return InputManagerVR.Instance.OculusQuestRightControllerPrefab;
            }
        }
        else if (deviceNameLower.Contains("touch"))
        {
            if (deviceNameLower.Contains("left"))
            {
                return InputManagerVR.Instance.OculusTouchLeftControllerPrefab;
            }
            else
            {
                return InputManagerVR.Instance.OculusTouchRightControllerPrefab;
            }
        }
        else if (deviceNameLower.Contains("knuckles"))
        {
            if (deviceNameLower.Contains("left"))
            {
                return InputManagerVR.Instance.ValveIndexLeftControllerPrefab;
            }
            else
            {
                return InputManagerVR.Instance.ValveIndexRightControllerPrefab;
            }
        }
        else if (deviceNameLower.Contains("vive"))
        {
            if (deviceNameLower.Contains("left"))
            {
                return InputManagerVR.Instance.ViveLeftControllerPrefab;
            }
            else
            {
                return InputManagerVR.Instance.ViveRightControllerPrefab;
            }
        }
        else
        {
            return InputManagerVR.Instance.DefaultControllerPrefab;
        }
    }
}
