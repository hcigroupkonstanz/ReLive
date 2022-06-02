using System.Collections.Generic;

namespace Relive.Data
{
    public class Event
    {
        public string eventId;
        public List<string> entityIds;
        public string sessionId;
        public string eventType;
        public long timestamp;
        public long endTime;
        public string message;
        public string name;
        public List<Attachment> attachments;
        public JsonVector3 position = null;
    }
}