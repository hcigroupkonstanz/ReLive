namespace ReLive.Core
{
    public enum StateType { Entity, Event }

    public class State : SyncedObject
    {
        public string ParentId;
        public string SessionId;
        public StateType StateType;

        public long Timestamp;
        protected override string GetChannel() => "states";


        protected override void SetIdData()
        {
            SetData("parentId", ParentId);
            SetData("sessionId", SessionId);
            SetData("stateType", StateType.ToString().ToLower());
            SetData("timestamp", Now);
        }
    }
}
