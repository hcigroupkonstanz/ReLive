using HCIKonstanz.Colibri;
using HCIKonstanz.Colibri.Core;
using HCIKonstanz.Colibri.Sync;
using Newtonsoft.Json.Linq;
using Relive.Data;
using UniRx;
using UnityEngine;

namespace Relive.Sync
{
    public class NotebookManager : SingletonBehaviour<NotebookManager>, IModelListener
    {
        public readonly Notebook Notebook = new Notebook();
        public readonly BehaviorSubject<bool> IsInitialized = new BehaviorSubject<bool>(false);

        protected override void Awake()
        {
            base.Awake();

            if (SyncConfiguration.Current.EnableSync)
            {
                SyncCommands.Instance.AddModelListener(this);
                Notebook.OnLocalUpdate += OnLocalUpdate;
            }
        }

        private void OnLocalUpdate(Notebook t)
        {
            SyncCommands.Instance.SendModelUpdate(GetChannelName(), JObject.FromObject(Notebook));
        }

        public string GetChannelName() => "notebook";

        public void OnSyncInitialized(JArray initialNotebooks)
        {
            // should only be one
            OnSyncUpdate(initialNotebooks[0] as JObject);

            Debug.Log($"Fetched current notebook from server");
            IsInitialized.OnNext(true);
        }


        public void OnSyncUpdate(JObject update)
        {
            var notebookUpdate = update.ToObject<Notebook>();
            Notebook.Name = notebookUpdate.Name;
            Notebook.IsPaused = notebookUpdate.IsPaused;
            Notebook.PlaybackTimeSeconds = notebookUpdate.PlaybackTimeSeconds;
            Notebook.PlaybackSpeed = notebookUpdate.PlaybackSpeed;
            Notebook.SceneView = notebookUpdate.SceneView;

            foreach (var kv in notebookUpdate.SceneViewOptions)
            {
                if (!Notebook.SceneViewOptions.ContainsKey(kv.Key))
                    Notebook.SceneViewOptions.Add(kv.Key, kv.Value);
                else
                    Notebook.SceneViewOptions[kv.Key] = kv.Value;
            }

            Notebook.UpdateRemote();
        }

        public void OnSyncDelete(JObject deletedObject)
        {
            Debug.LogWarning("Unsupported notebook delete");
        }
    }
}
