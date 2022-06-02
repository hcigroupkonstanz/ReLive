using UnityEngine;
using TMPro;
using Relive.Data;
using Relive.Playback;

public class EventHoverInfo : MonoBehaviour
{
    public EventGameObject EventGameObject
    {
        get { return eventGameObject; }
        set
        {
            eventGameObject = value;
            eventGameObjectCollider = eventGameObject.gameObject.GetComponent<Collider>();
            if (eventGameObject.Event.message != null && eventGameObject.Event.message != "")
            {
                MessageText.text = "Message: " + eventGameObject.Event.message;
                MessageText.gameObject.SetActive(true);
            }
            else
            {
                MessageText.gameObject.SetActive(false);
            }

        }
    }
    private EventGameObject eventGameObject;
    private Collider eventGameObjectCollider;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI MessageText;

    void Update()
    {
        if (EventGameObject)
        {
            string eventName = eventGameObject.Event.name;
            if (eventName == null) eventName = eventGameObject.Event.eventType;
            if (PlaybackManager.Instance.PlaybackSessionCount > 1)
            {
                NameText.text = eventName + " (" + eventGameObject.PlaybackSession.LoadedSession.Session.name + ")";
            }
            else
            {
                NameText.text = eventName;
            }
            if (eventGameObjectCollider)
            {
                transform.position = new Vector3(eventGameObjectCollider.bounds.center.x, eventGameObjectCollider.bounds.max.y + 0.1f, eventGameObjectCollider.bounds.center.z);
            }
            else
            {
                transform.position = EventGameObject.gameObject.transform.position;
            }
        }
        // transform.forward = XRCamera.Instance.transform.forward;
        transform.forward = Vector3.Normalize(XRCamera.Instance.transform.position - transform.position) * -1f;
        float distanceScale = 0.00075f * Vector3.Distance(transform.position, XRCamera.Instance.transform.position);
        transform.localScale = new Vector3(distanceScale, distanceScale, 1f);
    }
}
