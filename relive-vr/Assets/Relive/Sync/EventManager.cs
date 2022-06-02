using HCIKonstanz.Colibri.Core;
using HCIKonstanz.Colibri.Sync;
using Newtonsoft.Json.Linq;
using Relive.Playback.Data;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Relive.Sync
{
    public class EventManager : SingletonBehaviour<EventManager>, IModelListener
    {
        public readonly List<Data.Event> Events = new List<Data.Event>();
        public readonly BehaviorSubject<bool> IsInitialized = new BehaviorSubject<bool>(false);

        protected override void Awake()
        {
            base.Awake();
            StudyManager.Instance.RegisterStudyListener(this);
        }

        public string GetChannelName() => "events";

        public void OnSyncInitialized(JArray initialEvent)
        {
            foreach (var ev in initialEvent)
            {
                Events.Add(ev.ToObject<Data.Event>());
            }

            Debug.Log($"Fetched {Events.Count} events from server");
            IsInitialized.OnNext(true);
        }


        public void OnSyncUpdate(JObject update)
        {
            var eventUpdate = update.ToObject<Data.Event>();
            var localEvent = Events.FirstOrDefault(e => e.sessionId == eventUpdate.sessionId && e.eventId == eventUpdate.eventId);

            if (localEvent != null)
            {
                // update local event
                // TODO: probably minor object refactoring needed
            }
            else
            {
                Debug.Log($"Adding new event {eventUpdate.eventId} due to remote update");
                Events.Add(eventUpdate);
            }
        }

        public void OnSyncDelete(JObject deletedObject)
        {
            var deletedEvent = deletedObject.ToObject<Data.Event>();
            var removedEvents = Events.RemoveAll(ev => ev.eventId == deletedEvent.eventId && ev.sessionId == deletedEvent.sessionId);
            Debug.Log($"Removed {removedEvents} events due to remote upate");
        }

        public void AddEvent(Data.Event ev)
        {
            foreach (var dp in DataPlayer.ActiveDataPlayers)
            {
                if (dp.PlaybackSession.LoadedSession.Session.sessionId == ev.sessionId)
                {
                    dp.CreateEventGameObject(ev);
                }
            }

            SyncCommands.Instance.SendModelUpdate(GetChannelName(), JObject.FromObject(ev));
        }
    }
}
