using ReLive.Core;
using System;

namespace ReLive.Sessions
{
    [Serializable]
    public class Session : SyncedObject
    {
        private string sessionId;

        public string Name
        {
            set { SetData("name", value); }
        }

        public string Description
        {
            set { SetData("description", value); }
        }


        public long StartTime
        {
            set { SetData("startTime", value); }
        }

        public long EndTime
        {
            set { SetData("endTime", value); }
        }

        protected override string GetChannel() => "sessions";

        public Session(string sessionId)
        {
            this.sessionId = sessionId;
        }

        public void Start()
        {
            StartTime = Now;
            EndTime = -1;

            ScheduleChanges();
        }

        public void End()
        {
            EndTime = Now;
            CommitChanges();
        }

        protected override void SetIdData()
        {
            SetData("sessionId", sessionId);
        }
    }
}
