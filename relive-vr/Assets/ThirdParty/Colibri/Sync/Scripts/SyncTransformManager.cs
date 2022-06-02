using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace HCIKonstanz.Colibri.Sync
{
    [DefaultExecutionOrder(-1000)]
    public class SyncTransformManager : MonoBehaviour
    {
        public GameObject Template;
        private readonly List<SyncTransform> _existingTransforms = new List<SyncTransform>();

        private void OnEnable()
        {
            var existingTransforms = FindObjectsOfType<SyncTransform>();
            _existingTransforms.AddRange(existingTransforms);

            //SyncCommands.Instance.AddModelUpdateListener(SyncTransform.CHANNEL, OnModelUpdate);
            //SyncTransform.ModelCreated()
            //    .TakeUntilDisable(this)
            //    .Where(m => !_existingTransforms.Any(e => e.Id == m.Id))
            //    .Subscribe(m =>
            //    {
            //        _existingTransforms.Add(m);
            //        m.TriggerSync();
            //    });

            //SyncTransform.ModelDestroyed()
            //    .TakeUntilDisable(this)
            //    .Subscribe(m => _existingTransforms.Remove(m));
        }

        private void OnDisable()
        {
            //SyncCommands.Instance.RemoveModelUpdateListener(SyncTransform.CHANNEL, OnModelUpdate);
        }

        private void OnModelUpdate(JObject data)
        {
            var id = data["Id"].Value<int>();
            if (!_existingTransforms.Any(t => t.Id == id))
            {
                Template.SetActive(false);
                var go = Instantiate(Template);
                var syncTransform = go.AddComponent<SyncTransform>();
                syncTransform.Id = id;
                _existingTransforms.Add(syncTransform);
                go.SetActive(true);
            }
        }
    }
}
