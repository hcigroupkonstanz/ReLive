using ReLive.Sessions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UniRx;

namespace ReLive.Core
{
    public abstract class Traceable : SyncedObject
    {
        public struct AttachmentMessage
        {
            // entity or event ID
            public string Id;
            public string SessionId;
            public string Channel;
            public string Attachment;
        }

        public struct Attachment
        {
            public string id;
            public string type;
            public string content;
        }


        protected State currentState = new State();

        private static readonly Subject<AttachmentMessage> attachmentUpdates = new Subject<AttachmentMessage>();
        public static readonly IObservable<AttachmentMessage> AttachmentUpdates = attachmentUpdates.AsObservable();

        private List<Attachment> attachments = new List<Attachment>();

        protected abstract string GetId();

        protected void InitializeTracing(string traceableId, string sessionId, StateType type)
        {
            currentState.StateType = type;
            currentState.ParentId = traceableId;
            currentState.SessionId = sessionId;
        }

        public void AddTrace(Dictionary<string, object> properties)
        {
            currentState.SetData(properties);
        }

        public void AddTrace(string propertyName, object propertyValue)
        {
            currentState.SetData(propertyName, propertyValue);
        }

        public void AttachContent(string contentId, string type, string content)
        {
            attachments.Add(new Attachment
            {
                id = contentId,
                type = type,
                content = content
            });
        }

        public void DetachContent(string id)
        {
            throw new NotImplementedException();
        }

        public override void ScheduleChanges()
        {
            base.ScheduleChanges();
            currentState.ScheduleChanges();
        }

        protected override void CommitChanges()
        {
            base.CommitChanges();

            foreach (var attachment in attachments)
            {
                // TODO: inconsistent with other methods of sending data to server
                attachmentUpdates.OnNext(new AttachmentMessage
                {
                    Id = GetId(),
                    // TODO: assumes that session is running
                    SessionId = SessionController.Instance.SessionId,
                    // TODO: can this be integrated into main channel?
                    Channel = "attachments-" + GetChannel(),
                    Attachment = JsonConvert.SerializeObject(attachment)
                });
            }

            // TODO: should mark attachments as sent, so that "detachContent" can be implemented
            attachments.Clear();
        }
    }
}
