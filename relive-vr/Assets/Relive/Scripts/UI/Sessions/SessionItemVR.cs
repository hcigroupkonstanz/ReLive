using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Relive.Data;
using System;

namespace Relive.UI.Sessions
{
    public class SessionItemVR : MonoBehaviour
    {
        private Session session;

        public Session Session
        {
            get { return session; }
            set
            {
                if (session != null)
                    session.OnUpdate -= OnSessionUpdate;

                session = value;
                SessionName.text = session.name;
                SessionDescription.text = session.description;
                SessionDate.text = "Date: " + DateTimeOffset.FromUnixTimeMilliseconds(session.startTime).UtcDateTime.ToString();
                SessionDuration.text = "Length: " + TimeSpan
                    .FromSeconds((session.endTime - session.startTime) / 1000.0f)
                    .ToString(@"hh\:mm\:ss\:fff");

                session.OnUpdate += OnSessionUpdate;
                SetAsActive(session.IsActive);
            }
        }

        public Toggle SessionEnabledToggle;
        public TextMeshProUGUI SessionName;
        public TextMeshProUGUI SessionDate;
        public TextMeshProUGUI SessionDescription;
        public TextMeshProUGUI SessionDuration;
        public Image StatusImage;
        public Sprite OnSprite;
        public Sprite OffSprite;

        private void Awake()
        {
            SessionEnabledToggle.onValueChanged.AddListener(state =>
            {
                session.IsActive = state;
                session.UpdateLocal();
                if (state)
                {
                    StatusImage.sprite = OnSprite;
                }
                else
                {
                    StatusImage.sprite = OffSprite;
                }
            });
        }

        private void OnDestroy()
        {
            // TODO: might not be necessary?
            if (session != null)
                session.OnUpdate -= OnSessionUpdate;
        }

        private void OnSessionUpdate(Session s)
        {
            SetAsActive(s.IsActive);
        }


        public void SetAsActive(bool status)
        {
            SessionEnabledToggle.SetIsOnWithoutNotify(status);
            if (status)
            {
                StatusImage.sprite = OnSprite;
            }
            else
            {
                StatusImage.sprite = OffSprite;
            }
        }

    }
}