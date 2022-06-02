using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Relive.Data;
using Relive.Playback;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Playback
{
    public class PlaybackSessionsPanelVR : MonoBehaviour
    {
        public static PlaybackSessionsPanelVR Instance;
        public Slider GlobalPlayBackSlider;
        private Dictionary<PlaybackSession, PlaybackSessionSlider> playbackSessionSliders;
        public GameObject PlayBackSessionSliderPrefab;

        public GameObject SessionsPanel;
        public GameObject ContentPanel;
        public RectTransform DragPlane;


        public Image PinnedImage;
        public Sprite PinnedSprite;
        public Sprite UnpinnedSprite;
        private RadialView radialView;
        private BoxCollider collider;


        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            radialView = GetComponent<RadialView>();
            collider = GetComponent<BoxCollider>();
            playbackSessionSliders = new Dictionary<PlaybackSession, PlaybackSessionSlider>();

            ColorBlock cb = GlobalPlayBackSlider.colors;
            cb.normalColor = new Color32(0, 0, 0, 255);
            cb.highlightedColor = new Color32(0, 208, 255, 255);
            cb.selectedColor = new Color32(0, 0, 0, 255);
            cb.pressedColor = new Color32(0, 208, 255, 255);
            GlobalPlayBackSlider.colors = cb;
        }

        private void Update()
        {
            // Recalculate Height of Content Panel
            // TODO this is a workaround as content size fitter on Content Panel does not resize when height of Sessions Panel is updated
            RectTransform ContentPanelRectTransform = ContentPanel.GetComponent<RectTransform>();
            ContentPanelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                SessionsPanel.GetComponent<RectTransform>().sizeDelta.y);
        }

        // Update is called once per frame
        public void UpdateSliderValue(float seconds)
        {
            // GlobalPlayBackSlider.value = seconds;
            GlobalPlayBackSlider.SetValueWithoutNotify(seconds);
        }

        public void OnSliderValueChanged(float value)
        {
            PlaybackManager.Instance.JumpTo(value);
        }

        public float GetSecondsForPositionOnSlider(PlaybackSession session)
        {
            PlaybackSessionSlider currentSlider = playbackSessionSliders[session];
            float currentSliderXPosition = currentSlider.RectTransform.anchoredPosition.x;

            float PositionInSeconds = currentSliderXPosition / GetComponent<RectTransform>().sizeDelta.x *
                                      GlobalPlayBackSlider.maxValue;

            return PositionInSeconds;
        }


        public void SliderWasDragged(GameObject ownObject)
        {
            PlaybackSessionSlider slider = ownObject.GetComponentInParent<PlaybackSessionSlider>();
            slider.startPointInSeconds = GetSecondsForPositionOnSlider(slider.PlaybackSession);
            UpdateSliders();
        }

        public void AddSliderWithPosition(PlaybackSession session, float positionInSeconds)
        {
            // Create Session Slider
            PlaybackSessionSlider playbackSession = Instantiate(PlayBackSessionSliderPrefab, SessionsPanel.transform)
                .GetComponentInChildren<PlaybackSessionSlider>();
            playbackSession.startPointInSeconds = positionInSeconds;
            playbackSessionSliders.Add(session, playbackSession);
            playbackSession.PlaybackSession = session;
            playbackSession.MovableElement.DragPlane = DragPlane;
            playbackSession.MovableElement.WasMoved.AddListener(SliderWasDragged);
            playbackSession.AddCriticalIncident(session);

            UpdateSliders();
        }

        public void UpdateSliders()
        {
            // Set Zoom Level
            CalculateSliderMaxValue();

            RecalculateRectTransforms();

            // Get longest session length
            MoveAllSlidersToPositions();
        }

        private void CalculateSliderMaxValue()
        {
            float maximumValue = float.MinValue;
            foreach (PlaybackSessionSlider slider in playbackSessionSliders.Values)
            {
                float currentValue = slider.PlaybackSession.PlaybackLengthSeconds + slider.startPointInSeconds;
                if (currentValue > maximumValue)
                {
                    maximumValue = currentValue;
                }
            }

            GlobalPlayBackSlider.maxValue = maximumValue;
        }

        private void MoveAllSlidersToPositions()
        {
            foreach (PlaybackSessionSlider playbackSessionSlider in playbackSessionSliders.Values)
            {
                // Move Layout Group padding so Layout Group can control child positions and sizes
                playbackSessionSlider.GetComponent<HorizontalLayoutGroup>().padding.left = (int)
                    (playbackSessionSlider.startPointInSeconds /
                    GlobalPlayBackSlider.maxValue * GetComponent<RectTransform>().rect.width);
            }
        }

        public void AddSlider(PlaybackSession session)
        {
            // Create Session Slider
            PlaybackSessionSlider playbackSession = Instantiate(PlayBackSessionSliderPrefab, SessionsPanel.transform)
                .GetComponentInChildren<PlaybackSessionSlider>();
            playbackSessionSliders.Add(session, playbackSession);
            playbackSession.PlaybackSession = session;
            playbackSession.MovableElement.DragPlane = DragPlane;
            playbackSession.MovableElement.WasMoved.AddListener(SliderWasDragged);
            playbackSession.AddCriticalIncident(session);

            UpdateSliders();
        }

        private void RecalculateRectTransforms()
        {
            // Recalculate Widths of each session
            RectTransform panelRectTransform = GetComponent<RectTransform>();
            foreach (PlaybackSessionSlider playbackSessionSlider in playbackSessionSliders.Values)
            {
                playbackSessionSlider.MovableElement.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                    playbackSessionSlider.PlaybackSession.PlaybackLengthSeconds /
                    GlobalPlayBackSlider.maxValue *
                    panelRectTransform.rect.width);
            }
        }

        public void RemoveSlider(PlaybackSession session)
        {
            Destroy(playbackSessionSliders[session].gameObject);
            playbackSessionSliders.Remove(session);

            UpdateSliders();
        }

        public void SetRadialView()
        {
            bool enable = !collider.enabled;
            radialView.CalculatePosition = !enable;
            collider.enabled = enable;
            if (enable)
            {
                PinnedImage.sprite = PinnedSprite;
            }

            else
            {
                PinnedImage.sprite = UnpinnedSprite;
            }
        }
    }
}