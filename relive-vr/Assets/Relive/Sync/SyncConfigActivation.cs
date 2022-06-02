using HCIKonstanz.Colibri;
using UnityEngine;

namespace Relive.RenderStreaming
{
    [DefaultExecutionOrder(-1000)]
    public class SyncConfigActivation : MonoBehaviour
    {
        private void Awake()
        {
            var config = SyncConfiguration.Current;
            if (!config.EnableSync)
                Destroy(gameObject);
        }
    }
}
