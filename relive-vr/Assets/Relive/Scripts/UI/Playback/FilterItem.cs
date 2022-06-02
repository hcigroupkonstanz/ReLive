using System.Collections.Generic;
using Relive.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Playback
{
    public class FilterItem : MonoBehaviour
    {
        // FIXME: workaround for syncing
        public static readonly List<FilterItem> ActiveItems = new List<FilterItem>();
        public delegate void FilterUpdateDelegate(FilterItem item);
        public static event FilterUpdateDelegate FilterUpdate;

        [HideInInspector]
        public bool IsVisible;

        public Toggle VisibleToggle;
        public Image VisibleToggleImage;
        public Sprite ShowSprite;
        public Sprite HideSprite;
        public string EventType;
        private string sessionId;
        private PlaybackSession playbackSession;
        public PlaybackSession PlaybackSession
        {
            get { return this.playbackSession; }
            set
            {
                this.playbackSession = value;
                sessionId = playbackSession.LoadedSession.Session.sessionId;
                UpdateUI();
            }
        }

        private Dictionary<string, bool> EventFilter => playbackSession.LoadedSession.Session.EventFilters;

        private void Awake()
        {
            ActiveItems.Add(this);
        }

        private void OnDestroy()
        {
            ActiveItems.Remove(this);
        }

        public void OnVisibleToggleChanged(bool visible)
        {
            IsVisible = visible;
            if (visible)
            {
                foreach (EventGameObject eventGameObject in EventGameObject.ActiveEvents)
                {
                    if (eventGameObject.Event.sessionId == sessionId && eventGameObject.Event.eventType == EventType)
                    {
                        eventGameObject.Hide = false;
                    }
                }
                VisibleToggleImage.sprite = ShowSprite;
            }
            else
            {
                foreach (EventGameObject eventGameObject in EventGameObject.ActiveEvents)
                {
                    if (eventGameObject.Event.sessionId == sessionId && eventGameObject.Event.eventType == EventType)
                    {
                        eventGameObject.Hide = true;
                    }
                }
                VisibleToggleImage.sprite = HideSprite;
            }

            if (!EventFilter.ContainsKey(EventType))
                EventFilter.Add(EventType, visible);
            else
                EventFilter[EventType] = visible;

            // FIXME: quick workaround for different STREAM events
            if (EventType == "click")
            {
                if (!EventFilter.ContainsKey("voice"))
                    EventFilter.Add("voice", visible);
                else
                    EventFilter["voice"] = visible;

                if (!EventFilter.ContainsKey("action"))
                    EventFilter.Add("action", visible);
                else
                    EventFilter["action"] = visible;
            }

            FilterUpdate.Invoke(this);
        }

        public void UpdateUI()
        {
            bool filtered = EventFilter.ContainsKey(EventType) && !EventFilter[EventType];
            VisibleToggle.SetIsOnWithoutNotify(!filtered);
            if (filtered)
            {
                VisibleToggleImage.sprite = HideSprite;
            }
            else
            {
                VisibleToggleImage.sprite = ShowSprite;
            }
        }
    }
}
