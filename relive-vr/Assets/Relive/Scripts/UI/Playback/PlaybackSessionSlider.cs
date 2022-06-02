using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Relive.Data;
using Relive.Tools;
using UnityEngine.UI;
using Event = Relive.Data.Event;
using Relive.UI.Playback;

public class PlaybackSessionSlider : MonoBehaviour
{
    public PlaybackSession PlaybackSession
    {
        set
        {
            playbackSession = value;
            NameText.text = playbackSession.LoadedSession.Session.name;
        }
        get { return playbackSession; }
    }

    public MovableUIElement MovableElement;
    private PlaybackSession playbackSession;
    public TextMeshProUGUI PlaybackTimeText;
    public TextMeshProUGUI DurationTimeText;
    public TextMeshProUGUI NameText;
    public RectTransform RectTransform;
    public float startPointInSeconds;
    private bool sliderInitialized = false;
    public List<CriticalIncidentLayer> CriticalIncidentLayers;
    public GameObject CriticalIncidentsContainer;
    public Image CriticalIncidentsContainerImage;

    public CriticalIncident CriticalIncidentPrefab;
    public CriticalIncidentLayer CriticalIncidentLayerPrefab;

    public float CriticalIncidentMaxHeight = 30;
    public float CriticalIncidentMinHeight = 5;

    private bool isMaximized;

    private void Awake()
    {
        CriticalIncidentLayers = new List<CriticalIncidentLayer>();
    }

    private void Start()
    {
        if (MovableElement) MovableElement.IsSelected.AddListener(OnSelect);
    }

    public void OnSelect(GameObject GO)
    {
        isMaximized = !isMaximized;
        if (isMaximized)
            MaximizeLayer();
        else
            MinimizeLayer();
    }

    void Update()
    {
        if (PlaybackSession != null)
        {
            PlaybackTimeText.text =
                TimeSpan.FromSeconds(PlaybackSession.PlaybackTimeSeconds).ToString(@"hh\:mm\:ss\:fff");
            DurationTimeText.text =
                TimeSpan.FromSeconds(PlaybackSession.PlaybackLengthSeconds).ToString(@"hh\:mm\:ss\:fff");

            if (PlaybackSession.ShowOutline && CriticalIncidentsContainerImage.color == Color.white)
            // if (PlaybackSession.ShowOutline && CriticalIncidentsContainerImage.color != PlaybackSession.SessionColor)
            {
                Color color = PlaybackSession.SessionColor;
                color.a = 0.75f;
                CriticalIncidentsContainerImage.color = color;

                Color32 color32 = (Color32)color;
                //Color result text white or black depending on background
                if ((color.r * 0.299f + color.g * 0.587f + color.b * 0.114f) > 186)
                {
                    NameText.color = new Color(0, 0, 0, 255);
                    PlaybackTimeText.color = new Color(0, 0, 0, 255);
                    DurationTimeText.color = new Color(0, 0, 0, 255);
                }
                else
                {
                    NameText.color = new Color(255, 255, 255, 255);
                    PlaybackTimeText.color = new Color(255, 255, 255, 255);
                    DurationTimeText.color = new Color(255, 255, 255, 255);
                }
            }
            else if (!PlaybackSession.ShowOutline && CriticalIncidentsContainerImage.color != Color.white)
            {
                CriticalIncidentsContainerImage.color = Color.white;
                NameText.color = new Color(0, 0, 0, 255);
                PlaybackTimeText.color = new Color(0, 0, 0, 255);
                DurationTimeText.color = new Color(0, 0, 0, 255);
            }
        }
    }

    public void AddCriticalIncident(PlaybackSession session)
    {
        if (session.LoadedSession.Events.Count > 0)
        {
            // foreach (Event incidentEvent in session.LoadedSession.Events)
            // {
            //     CriticalIncidentLayer currentLayer = Instantiate(CriticalIncidentLayerPrefab,
            //         CriticalIncidentsContainer.transform);
            //     CriticalIncidentLayers.Add(currentLayer);
            //     CriticalIncident incident = Instantiate(CriticalIncidentPrefab, currentLayer.transform);
            //     incident.IncidentElement.incidentRange.DragPlane = MovableElement.DragPlane;
            //     incident.SetEvent(incidentEvent);
            //     float stamp = (incidentEvent.timestamp - session.LoadedSession.Session.startTime) / 1000f;
            //     // Debug.Log(stamp);

            //     //TODO remove example end values
            //     incident.SetTransform(
            //         CalculateNormalizedIncidentPosition(stamp, session.PlaybackLengthSeconds),
            //         CalculateNormalizedIncidentPosition(stamp + 180, session.PlaybackLengthSeconds));
            //     currentLayer.addIncident(incident);
            // }
        }

        MinimizeLayer();
    }

    // private float CalculateNormalizedIncidentPosition(float incidentEventTimestamp, float sessionPlaybackLengthSeconds)
    // {
    //     return incidentEventTimestamp / sessionPlaybackLengthSeconds;
    // }

    public void MaximizeLayer()
    {
        foreach (CriticalIncidentLayer criticalIncidentLayer in CriticalIncidentLayers)
        {
            criticalIncidentLayer.GetComponent<RectTransform>()
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CriticalIncidentMaxHeight);
            criticalIncidentLayer.MaximizeLayer();
        }

        StartCoroutine(ResizeCoroutine());
    }

    //TODO this is a terrible workaround that fixes Unitys weird UI system issues. The issue is that Unity
    //Unity only begins to recalculate the preferred size of a content size fitter if the variable is changed
    //Need to look for way to manually update it
    IEnumerator ResizeCoroutine()
    {
        yield return new WaitForEndOfFrame();
        ContentSizeFitter fitter = CriticalIncidentsContainer.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.GetComponent<RectTransform>().ForceUpdateRectTransforms();
        yield return new WaitForEndOfFrame();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.GetComponent<RectTransform>().ForceUpdateRectTransforms();
        yield return new WaitForEndOfFrame();
        fitter = gameObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.GetComponent<RectTransform>().ForceUpdateRectTransforms();
        yield return new WaitForEndOfFrame();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.GetComponent<RectTransform>().ForceUpdateRectTransforms();
        yield return new WaitForEndOfFrame();
        ContentSizeFitter[] contentSizeFitters = transform.GetComponentsInParent<ContentSizeFitter>();
        for (int i = 0; i < contentSizeFitters.Length; i++)
        {
            contentSizeFitters[i].verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.GetComponent<RectTransform>().ForceUpdateRectTransforms();
            yield return new WaitForEndOfFrame();
            contentSizeFitters[i].verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.GetComponent<RectTransform>().ForceUpdateRectTransforms();
            yield return new WaitForEndOfFrame();
        }
    }

    public void MinimizeLayer()
    {
        foreach (CriticalIncidentLayer criticalIncidentLayer in CriticalIncidentLayers)
        {
            criticalIncidentLayer.GetComponent<RectTransform>()
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CriticalIncidentMinHeight);
            criticalIncidentLayer.MinimizeLayer();
        }

        StartCoroutine(ResizeCoroutine());
    }

    public void OnFilterButtonClicked()
    {
        if (FilterPanel.Instance.gameObject.activeSelf && FilterPanel.Instance.PlaybackSession == PlaybackSession)
        {
            FilterPanel.Instance.CloseFilterPanel();
        }
        else
        {
            FilterPanel.Instance.OpenFilterPanel(PlaybackSession, transform.GetSiblingIndex());
        }

    }
}