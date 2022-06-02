using System;

namespace ReLive.Events
{
    public enum MessageType { Debug, Info, Warning, Error, Exception }

    public class MessageEvent : ReliveEvent
    {
        public MessageEvent(MessageType logType, string message, string stacktrace = null)
            : base(Guid.NewGuid().ToString(), ReliveEventType.Log)
        {
            SetData("logType", logType.ToString().ToLower());
            SetData("logMessage", message);

            if (stacktrace != null)
                SetData("stacktrace", stacktrace);
        }
    }
}
