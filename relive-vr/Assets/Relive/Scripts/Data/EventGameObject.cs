using System.Collections;
using System.Collections.Generic;
using Relive.Playback.Data;
using UnityEngine;

namespace Relive.Data
{
    public class EventGameObject : MonoBehaviour
    {
        // FIXME: workaround copied from EntityGameObject
        public static readonly List<EventGameObject> ActiveEvents = new List<EventGameObject>();


        // Data
        private Relive.Data.Event _event;
        public Relive.Data.Event Event
        {
            get => _event;
            set
            {
                _event = value;
            }
        }
        public List<State> States;
        private PlaybackSession playbackSession;
        public PlaybackSession PlaybackSession
        {
            get => playbackSession;
            set
            {
                playbackSession = value;
                var filters = playbackSession.LoadedSession.Session.EventFilters;
                if (filters.TryGetValue(Event.eventType, out var show))
                    Hide = !show;
            }
        }
        private int playbackStateIndex = -1;


        // Visibility
        public delegate void VisibleChangedHandler(bool visible);
        public event VisibleChangedHandler OnVisibleChanged;


        // Selection
        public bool IsSelected = false;
        public delegate void SelectedChangedHandler(bool visible);
        public event SelectedChangedHandler OnSelectedChanged;


        // Outline
        private Outline outline;
        public static Color DEFAULT_OUTLINE_COLOR = Color.white;
        public static float DEFAULT_OUTLINE_WIDTH = 2;


        // Hover Info
        private EventHoverInfo hoverInfo;
        private bool showHoverInfoAlways = false;

        public int PlaybackStateIndex
        {
            get
            {
                // Prevent that playbackStateIndex produces an out of range exception at the end of a session
                if (States.Count > playbackStateIndex)
                {
                    return playbackStateIndex;
                }
                else
                {
                    return States.Count - 1;
                }
            }
            set
            {
                playbackStateIndex = value;
            }
        }

        public bool Visible
        {
            get { return gameObject.activeSelf; }
            set
            {
                if (!hide)
                {
                    gameObject.SetActive(value);
                }
            }
        }

        private bool hide = false;

        public bool Hide
        {
            get { return hide; }
            set
            {
                hide = value;
                if (hide)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    if (PlaybackStateIndex > -1)
                    {
                        gameObject.SetActive(true);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }


        // FIXME: workaround
        private void Awake()
        {
            ActiveEvents.Add(this);
        }

        private void OnDestroy()
        {
            ActiveEvents.Remove(this);
        }

        void Update()
        {
            if (PlaybackSession.ShowOutline && !IsSelected && (!outline || outline.OutlineColor != PlaybackSession.SessionColor || !outline.enabled))
            {
                ShowOutline(PlaybackSession.SessionColor);
            }
            else if (!PlaybackSession.ShowOutline)
            {
                if (!IsSelected)
                {
                    HideOutline();
                }
                else
                {
                    ShowDefaultOutline(true);
                }
            }

            // if (!showHoverInfoAlways && ShowAllEntityNamesButton.Instance.ShowAllEntityNames)
            // {
            //     ShowHoverInfoAlways();
            // }
            // else if (showHoverInfoAlways && !ShowAllEntityNamesButton.Instance.ShowAllEntityNames)
            // {
            //     DisableHoverInfoAlways();
            // }
        }

        public void SetVisible(bool value)
        {
            Visible = value;
            OnVisibleChanged?.Invoke(value);
        }

        private void ShowDefaultOutline(bool show)
        {
            if (!OutlineAllowed()) return;
            if (!outline) outline = gameObject.AddComponent<Outline>();
            outline.OutlineColor = DEFAULT_OUTLINE_COLOR;
            outline.OutlineWidth = DEFAULT_OUTLINE_WIDTH;
            outline.enabled = show;
        }

        public void ShowOutline(Color color)
        {
            if (!OutlineAllowed()) return;
            if (!outline) outline = gameObject.AddComponent<Outline>();
            outline.enabled = true;
            outline.OutlineColor = color;
        }

        public void HideOutline()
        {
            if (outline)
            {
                outline.enabled = false;
            }
        }

        private bool OutlineAllowed()
        {
            return true;
        }

        public void ShowHoverInfo()
        {
            if (showHoverInfoAlways) return;
            if (!gameObject.activeSelf) return;
            if (!hoverInfo)
            {
                GameObject hoverInfoGameObject = Instantiate(XRUIPrefabs.Instance.EventHoverInfoPrefab);
                hoverInfo = hoverInfoGameObject.GetComponent<EventHoverInfo>();
                hoverInfo.EventGameObject = this;
                if (Event.name != null)
                {
                    hoverInfoGameObject.transform.name = "HoverInfo " + Event.name;
                }
                else
                {
                    hoverInfoGameObject.transform.name = "HoverInfo " + Event.eventType;
                }

            }
            hoverInfo.gameObject.SetActive(true);
        }

        public void ShowHoverInfoAlways()
        {
            ShowHoverInfo();
            showHoverInfoAlways = true;
        }

        public void HideHoverInfo(bool force = false)
        {
            if (!force && showHoverInfoAlways) return;
            if (hoverInfo) hoverInfo.gameObject.SetActive(false);
        }

        public void DisableHoverInfoAlways()
        {
            showHoverInfoAlways = false;
            HideHoverInfo();
        }

        public void ChangeSelected()
        {
            IsSelected = !IsSelected;
            OnSelectedChanged?.Invoke(IsSelected);
        }

        public static EventGameObject IsEventGameObject(GameObject gameObject)
        {
            EventGameObject eventGameObject = null;
            if (eventGameObject = gameObject.GetComponent<EventGameObject>())
            {

            }
            else if (gameObject.transform.parent && gameObject.transform.parent.name != DataPlayerObjects.Instance.gameObject.name)
            {
                eventGameObject = gameObject.transform.parent.GetComponent<EventGameObject>();
            }
            return eventGameObject;
        }

    }
}
