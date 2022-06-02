using ReLive.Core;
using ReLive.Entities;
using ReLive.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace ReLive.Events
{
    public enum ReliveEventType { VideoStart, VideoEnd, Click, Touch, Log, CriticalIncident, TaskStart, TaskEnd, TaskUpdate, Custom, Task }

    public class ReliveEvent : Traceable
    {
        public readonly string EventId;
        private readonly string sessionId;
        private readonly ReliveEventType eventType;

        protected override string GetId() => EventId;
        protected override string GetChannel() => "events";

        protected override void SetIdData()
        {
            SetData("eventId", EventId);
            SetData("sessionId", sessionId);
            SetData("timestamp", Now);
            SetData("eventType", Enum.GetName(typeof(ReliveEventType), eventType).ToLower());
        }

        protected ReliveEvent(string id, ReliveEventType type)
        {
            EventId = id;
            eventType = type;

            // TODO: expects that session has already started
            sessionId = SessionController.Instance.SessionId;
        }


        public static ReliveEvent Create(ReliveEventType type)
        {
            var logEvent = new ReliveEvent(Guid.NewGuid().ToString(), type);
            logEvent.InitializeTracing(logEvent.EventId, logEvent.sessionId, StateType.Event);
            return logEvent;
        }


        public static void Log(ReliveEventType type, Dictionary<string, object> metadata = null, params Entity[] affectedEntities)
        {
            if (!SessionController.Instance.IsSessionActive) return;
            var ev = new ReliveEvent(Guid.NewGuid().ToString(), type);

            if (metadata != null)
                ev.SetData(metadata);

            if (affectedEntities.Length > 0)
                ev.SetData("EntityIds", affectedEntities.Select(e => e.EntityId).ToArray());

            ev.ScheduleChanges();
        }

    }
}
