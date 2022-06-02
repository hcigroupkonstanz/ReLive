using ReLive.Core;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

namespace ReLive.Logging
{
    [DefaultExecutionOrder(-1000)]
    public class DebugLogger : MonoBehaviour
    {
        private void OnEnable()
        {
            SyncedObject.Changes
                .TakeUntilDisable(this)
                .Subscribe(msg =>
                {
                    Debug.Log("Change: " + JsonConvert.SerializeObject(msg));
                });

            Traceable.AttachmentUpdates
                .TakeUntilDisable(this)
                .Subscribe(attachment =>
                {
                    Debug.Log("Attachment: " + JsonConvert.SerializeObject(attachment));
                });
        }
    }
}
