using HCIKonstanz.Colibri.Sync;
using Relive.Tools;
using UnityEngine;

namespace Relive.RenderStreaming
{
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(Camera))]
    public class RemoteScreenshot : MonoBehaviour
    {
        private bool takeScreenshot;

        private void OnEnable()
        {
            SyncCommands.Instance.AddBoolListener("take-screenshot", OnScreenshotCommand);
        }

        private void OnDisable()
        {
            SyncCommands.Instance.RemoveBoolListener("take-screenshot", OnScreenshotCommand);
        }

        private void LateUpdate()
        {
            if (takeScreenshot)
            {
                var cam = GetComponent<Camera>();
                CameraTool.TakeScreenshot(cam.targetTexture, cam);
                takeScreenshot = false;
            }
        }

        private void OnScreenshotCommand(bool _)
        {
            takeScreenshot = true;
        }
    }
}
