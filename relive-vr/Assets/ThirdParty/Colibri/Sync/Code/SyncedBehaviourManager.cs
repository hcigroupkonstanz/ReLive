using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace HCIKonstanz.Colibri.Sync
{
    public abstract class SyncedBehaviourManager<T> : MonoBehaviour
        where T : SyncedBehaviour<T>
    {
        public T Template;

        /**
         *  Must be same as corresponding model's channel
         */
        public abstract string Channel { get; }

        private readonly List<T> _existingObjects = new List<T>();

        private void OnEnable()
        {
            var existingBehaviours = FindObjectsOfType<T>();
            _existingObjects.AddRange(existingBehaviours);

            //SyncCommands.Instance.AddModelUpdateListener(Channel, OnModelUpdate);
            //SyncedBehaviour<T>.ModelCreated()
            //    .TakeUntilDisable(this)
            //    .Where(m => m is T)
            //    .Where(m => !_existingObjects.Any(e => e.Id == m.Id))
            //    .Subscribe(m =>
            //    {
            //        _existingObjects.Add(m as T);
            //        m.TriggerSync();
            //    });

            //SyncedBehaviour<T>.ModelDestroyed()
            //    .TakeUntilDisable(this)
            //    .Where(m => m is T)
            //    .Subscribe(m => _existingObjects.Remove(m as T));
        }

        private void OnDisable()
        {
            //SyncCommands.Instance.RemoveModelUpdateListener(Channel, OnModelUpdate);
        }

        private void OnModelUpdate(JObject data)
        {
            var id = data["Id"].Value<string>();
            if (!_existingObjects.Any(t => t.Id == id))
            {
                Template.enabled = false;
                var go = Instantiate(Template);
                go.Id = id;
                _existingObjects.Add(go);
                go.enabled = true;
            }
        }

    }
}
