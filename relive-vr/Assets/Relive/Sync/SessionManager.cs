using HCIKonstanz.Colibri.Core;
using HCIKonstanz.Colibri.Sync;
using Newtonsoft.Json.Linq;
using Relive.Data;
using Relive.UI.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Relive.Sync
{
    public class SessionManager : SingletonBehaviour<SessionManager>, IModelListener
    {
        public readonly List<Session> Sessions = new List<Session>();
        public readonly BehaviorSubject<bool> IsInitialized = new BehaviorSubject<bool>(false);

        protected override void Awake()
        {
            base.Awake();
            StudyManager.Instance.RegisterStudyListener(this);
            FilterItem.FilterUpdate += HandleFilterUpdate;
        }

        private void HandleFilterUpdate(FilterItem item)
        {
            var session = item.PlaybackSession.LoadedSession.Session;
            var type = item.EventType;
            if (session.EventFilters.ContainsKey(type))
            {
                session.EventFilters[type] = item.IsVisible;
                OnLocalUpate(session);
            }
        }

        public string GetChannelName() => "sessions";

        public void OnSyncInitialized(JArray initialSessions)
        {
            foreach (var sessionJson in initialSessions)
            {
                var session = sessionJson.ToObject<Session>();
                session.OnLocalUpdate += OnLocalUpate;
                Sessions.Add(session);
            }

            Debug.Log($"Fetched {Sessions.Count} sessions from server");
            IsInitialized.OnNext(true);
        }

        private void OnLocalUpate(Session s)
        {
            SyncCommands.Instance.SendModelUpdate(GetChannelName(), new JObject
            {
                { "sessionId", s.sessionId },
                { "isActive", s.IsActive },
                { "isExpanded", s.IsExpanded },
                { "eventFilters", JObject.FromObject(s.EventFilters) }
            });
        }

        public void OnSyncUpdate(JObject update)
        {
            var sessionUpdate = update.ToObject<Session>();
            var localSession = Sessions.FirstOrDefault(s => s.sessionId == sessionUpdate.sessionId);

            if (localSession != null)
            {
                localSession.IsActive = sessionUpdate.IsActive;
                localSession.IsExpanded = sessionUpdate.IsExpanded;

                foreach (var kv in sessionUpdate.EventFilters)
                {
                    if (!localSession.EventFilters.ContainsKey(kv.Key))
                        localSession.EventFilters.Add(kv.Key, kv.Value);
                    else
                        localSession.EventFilters[kv.Key] = kv.Value;

                    foreach (var filter in FilterItem.ActiveItems
                        .Where(f =>
                            f.PlaybackSession != null &&
                            f.PlaybackSession.LoadedSession.Session.sessionId == localSession.sessionId &&
                            f.EventType == kv.Key))
                    {
                        filter.UpdateUI();
                    }

                    var sessionId = localSession.sessionId;
                    foreach (EventGameObject eventGameObject in EventGameObject.ActiveEvents)
                    {
                        if (eventGameObject.Event.sessionId == sessionId && eventGameObject.Event.eventType == kv.Key)
                            eventGameObject.Hide = !kv.Value;
                    }
                }

                localSession.UpdateRemote();
            }
            else
            {
                Debug.LogWarning("Dynamically adding sessions is unsupported");
            }
        }

        public void OnSyncDelete(JObject deletedObject)
        {
            Debug.LogWarning("Dynamically removing sessions is unsupported");
        }
    }
}
