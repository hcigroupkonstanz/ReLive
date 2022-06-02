using HCIKonstanz.Colibri;
using UnityEngine;

namespace Relive.RenderStreaming
{
    [DefaultExecutionOrder(-1000)]
    public class RenderStreamingConfiguration : MonoBehaviour
    {
        private void Awake()
        {
            var config = SyncConfiguration.Current;
            var isRenderstreamingEnabled = config.EnableSync && config.EnableRenderstreaming && SystemInfo.graphicsDeviceVendor == "NVIDIA";

            if (isRenderstreamingEnabled)
            {
                var rsScript = GetComponentInChildren<Unity.RenderStreaming.RenderStreaming>();
                rsScript.urlSignaling = $"{config.Protocol}{config.ServerAddress}:{config.RestPort}/renderstreaming";
            }
            else
            {
                Destroy(gameObject);
            }

            if (config.EnableRenderstreaming && SystemInfo.graphicsDeviceVendor != "NVIDIA")
                Debug.LogError($"Unable to activate renderstreaming: Unsupported GPU vendor '{SystemInfo.graphicsDeviceVendor}'");
        }

    }
}
