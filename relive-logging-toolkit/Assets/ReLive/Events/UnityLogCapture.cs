using ReLive.Sessions;
using UniRx;
using UnityEngine;

namespace ReLive.Events
{
    public class UnityLogCapture : MonoBehaviour
    {
        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += OnLogMessage;
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= OnLogMessage;
        }

        private void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            MessageType logType;
            switch (type)
            {
                case LogType.Log:
                    logType = MessageType.Info;
                    break;

                case LogType.Warning:
                    logType = MessageType.Warning;
                    break;

                case LogType.Error:
                    logType = MessageType.Error;
                    break;

                case LogType.Exception:
                    logType = MessageType.Exception;
                    break;

                case LogType.Assert:
                default:
                    logType = MessageType.Debug;
                    break;
            }

            string stacktrace = null;
            if (type == LogType.Error || type == LogType.Exception)
                stacktrace = stackTrace;

            new MessageEvent(logType, condition, stacktrace).ScheduleChanges();
        }
    }

}
