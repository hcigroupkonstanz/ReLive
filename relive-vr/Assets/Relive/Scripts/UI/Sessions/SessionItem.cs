using Relive.Data;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Sessions
{
    public class SessionItem : MonoBehaviour
    {
        [HideInInspector]
        public SessionPanel SessionPanel;
        private Session session;
        public Session Session
        {
            get { return session; }
            set
            {
                session = value;
                SessionTitle.text = session.name;
                SessionDescription.text = session.description;
                SessionStartTime.text = DateTimeOffset.FromUnixTimeMilliseconds(session.startTime).UtcDateTime.ToString();
            }
        }
        public IDataProvider DataProvider;

        public Button SessionPlayButton;
        public Text SessionTitle;
        public Text SessionStartTime;
        public Text SessionDescription;

        void OnEnable()
        {
            SessionPlayButton.onClick.AddListener(OnSessionPlayButtonClicked);
        }

        void OnDisable()
        {
            SessionPlayButton.onClick.RemoveListener(OnSessionPlayButtonClicked);
        }

        public void OnSessionPlayButtonClicked()
        {
            Session.IsActive = true;
            Session.UpdateLocal();
            SessionPanel.Hide();
        }
    }
}
