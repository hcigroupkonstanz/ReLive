using ReLive.Utils;
using System;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace ReLive.Sessions
{
    public class SessionController : SingletonBehaviour<SessionController>
    {
        /// <summary>
        /// Automatically starts the session when the SessionController is loaded (Awake()).
        /// Disable this if you want to start the session manually
        /// </summary>
        public bool AutoStartSession = true;

        /// <summary>
        /// Automatically generates
        /// </summary>
        public bool AutoGenerateSessionId = true;

        /// <summary>
        /// (TODO: NYI) Automatically increment number of session id.
        /// </summary>
        [ConditionalField("AutoGenerateSessionId", true)]
        public bool AutoIncrementSessionId = false;

        /// <summary>
        /// Session ID. Cannot be changed after session has started.
        /// </summary>
        [ConditionalField("AutoGenerateSessionId", true)]
        public string SessionId;

        /// <summary>
        /// Descriptive session name for organization purposes.
        /// </summary>
        public string SessionName = "";

        /// <summary>
        /// Session description for organization purposes.
        /// </summary>
        public string SessionDescription = "";

        public bool IsSessionActive { get => session != null; }
        private Session session;

        private BehaviorSubject<string> sessionStart = new BehaviorSubject<string>(null);
        public IObservable<string> SessionStart { get => sessionStart.First(s => s != null); }

        private void OnEnable()
        {
            if (AutoStartSession)
                StartSession();
        }

        private void OnDisable()
        {
            StopSession();
        }

        public void StartSession()
        {
            if (IsSessionActive)
                return;

            if (AutoGenerateSessionId || string.IsNullOrEmpty(SessionId))
                SessionId = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(SessionName))
                SessionName = $"Session @ {DateTime.Now}";

            session = new Session(SessionId);
            session.Name = SessionName;
            session.Description = SessionDescription;
            session.Start();
            Debug.Log($"Starting new session: {SessionId}");

            sessionStart.OnNext(SessionId);
        }

        public void StopSession()
        {
            if (!IsSessionActive)
                return;

            Debug.Log($"Stopping session: {SessionId}");
            session.End();
            session = null;
        }

        public void AddMetadata(string key, object data)
        {
            // TODO: can only be used after StartSession()
            session.SetData(key, data);
        }
    }
}
