using System.Collections.Generic;
using UnityEngine;

public class CriticalIncidentLayer : MonoBehaviour
{
    private List<CriticalIncident> incidents;

    private void Awake()
    {
        incidents = new List<CriticalIncident>();
    }

    public void addIncident(CriticalIncident incident)
    {
        incidents.Add(incident);
    }

    public void MaximizeLayer()
    {
        foreach (CriticalIncident incident in incidents)
        {
            incident.IncidentElement.MaxSlider.gameObject.SetActive(true);
            incident.IncidentElement.MinSlider.gameObject.SetActive(true);
            incident.IncidentElement.incidentRange.enabled = true;
        }
    }
    
    public void MinimizeLayer()
    {
        foreach (CriticalIncident incident in incidents)
        {
            incident.IncidentElement.MaxSlider.gameObject.SetActive(false);
            incident.IncidentElement.MinSlider.gameObject.SetActive(false);
            incident.IncidentElement.incidentRange.enabled = false;
        }
    }
}
