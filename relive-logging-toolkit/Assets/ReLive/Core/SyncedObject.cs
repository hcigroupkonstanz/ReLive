using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ReLive.Core
{
    public struct SyncMessage
    {
        public string Channel;
        // TODO: serialize directly to byte[] ?
        public string Payload;
    }

    public abstract class SyncedObject
    {
        private static readonly Subject<SyncMessage> changes = new Subject<SyncMessage>();
        public static readonly IObservable<SyncMessage> Changes = changes.AsObservable();

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        protected static long Now { get => (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds; }

        private Dictionary<string, object> queuedChanges = new Dictionary<string, object>();

        private bool hasChanges = false;

        protected abstract string GetChannel();
        // TODO: needs a much better name
        protected abstract void SetIdData();

        public void SetData(Dictionary<string, object> values)
        {
            // TODO: could be more efficient?
            foreach (var kp in values)
            {
                if (kp.Value is Vector3)
                    SetData(kp.Key, (Vector3)kp.Value);
                else if (kp.Value is Quaternion)
                    SetData(kp.Key, (Quaternion)kp.Value);
                else
                    SetData(kp.Key, kp.Value);
            }
        }

        public void SetData(string key, Quaternion data)
        {
            SetData(key, new Dictionary<string, float>()
            {
                { "x", data.x },
                { "y", data.y },
                { "z", data.z },
                { "w", data.w },
            });
        }

        public void SetData(string key, Vector3 data)
        {
            SetData(key, new Dictionary<string, float>()
            {
                { "x", data.x },
                { "y", data.y },
                { "z", data.z },
            });
        }

        public void SetData(string key, object data)
        {
            // TODO: save directly as JSON / UTF8 / keep a list of rows, merge them in the ToJson()
            queuedChanges[key] = data;
            // TODO: is this necessary?
            hasChanges = true;
        }

        public async virtual void ScheduleChanges()
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            CommitChanges();
        }

        protected virtual void CommitChanges()
        {
            if (hasChanges)
            {
                SetIdData();
                var msg = new SyncMessage
                {
                    Channel = GetChannel(),
                    // TODO: convert directly to byte[]?
                    Payload = JsonConvert.SerializeObject(queuedChanges)
                };
                changes.OnNext(msg);

                queuedChanges.Clear();
                hasChanges = false;
            }
        }
    }
}
