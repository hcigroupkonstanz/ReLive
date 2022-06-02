using Relive.Tools;
using UnityEngine;
using UnityEngine.UI;
using Event = Relive.Data.Event;

public class CriticalIncident : MonoBehaviour
{
    private Event incidentEvent;
    
    public Tool Tool;

    public MinMaxSlider IncidentElement;
    
    public Image IncidentColorImage;

    private float normalizedXStart;
    private float normalizedXEnd;



    public void SetEvent(Event incidentEvent)
    {
        this.incidentEvent = incidentEvent;
    }

    public void SetTransform(float normalizedXStart, float normalizedXEnd)
    {
        this.normalizedXStart = normalizedXStart;
        this.normalizedXEnd = normalizedXEnd;
        IncidentElement.SetNormalizedMinMaxValues(normalizedXStart, normalizedXEnd);
    }

    private void Update()
    {
        UpdateColor();
    }
    public void UpdateColor()
    {
        //if (Tool != null)
        //{
        //    IncidentColorImage.color = Tool.Color;
        //}
    }

    public void Maximize()
    {
        IncidentElement.enabled = true;
    }

    public void Minimize()
    {
        IncidentElement.enabled = false;
    }
}