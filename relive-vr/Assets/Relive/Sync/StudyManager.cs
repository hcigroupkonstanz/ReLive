using HCIKonstanz.Colibri.Core;
using HCIKonstanz.Colibri.Sync;
using Newtonsoft.Json.Linq;
using Relive.Data;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Relive.Sync
{
    public class StudyManager : SingletonBehaviour<StudyManager>, IModelListener
    {
        public readonly List<Study> Studies = new List<Study>();
        public readonly BehaviorSubject<bool> IsInitialized = new BehaviorSubject<bool>(false);
        public readonly BehaviorSubject<Study> ActiveStudy = new BehaviorSubject<Study>(null);

        public void RegisterStudyListener(IModelListener listener)
        {
            var isSubscribed = false;
            ActiveStudy.Subscribe(study =>
            {
                if (study != null)
                {
                    if (!isSubscribed)
                    {
                        Debug.Log($"Listening to study {study.Name} on {listener.GetChannelName()}");
                        SyncCommands.Instance.AddModelListener(listener);
                        isSubscribed = true;
                    }
                    else
                    {
                        Debug.LogWarning("Warning: Switching studies is not supported");
                    }
                }
                else if (study == null)
                {
                    if (isSubscribed)
                    {
                        SyncCommands.Instance.RemoveModelListener(listener);
                        isSubscribed = false;
                        Debug.LogWarning("Warning: Switching studies is not supported");
                    }
                }
            });
        }

        private void OnEnable()
        {
            SyncCommands.Instance.AddModelListener(this);
        }

        private void OnDisable()
        {
            SyncCommands.Instance.RemoveModelListener(this);
        }

        public string GetChannelName() => "studies";

        public void OnSyncInitialized(JArray initialStudies)
        {
            foreach (var studyJson in initialStudies)
            {
                var study = studyJson.ToObject<Study>();
                Studies.Add(study);
                study.OnLocalUpdate += OnLocalUpdate;
                if (study.IsActive)
                {
                    Debug.Log("Loading already active study " + study.Name);
                    ActiveStudy.OnNext(study);
                }
            }

            Debug.Log($"Fetched {Studies.Count} studies from server");
            IsInitialized.OnNext(true);
        }

        private void OnLocalUpdate(Study t)
        {
            SyncCommands.Instance.SendModelUpdate(GetChannelName(), new JObject
            {
                { "name", t.Name },
                { "isActive", t.IsActive }
            });
        }

        public void OnSyncUpdate(JObject updatedObject)
        {
            var studyUpdate = updatedObject.ToObject<Study>();
            Debug.Log("Study update for " + studyUpdate.Name);
            var localStudy = Studies.FirstOrDefault(s => s.Name == studyUpdate.Name);

            localStudy.IsActive = studyUpdate.IsActive;

            if (localStudy.IsActive && ActiveStudy.Value?.Name != localStudy.Name)
            {
                Debug.Log("Initializing study " + localStudy.Name);
                ActiveStudy.OnNext(localStudy);
            }
        }

        public void OnSyncDelete(JObject deletedObject)
        {
            Debug.LogWarning("Unsupported study delete");
        }
    }
}
