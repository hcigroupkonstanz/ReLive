using UnityEngine;
using TMPro;
using Relive.Data;
using Relive.Playback;

public class EntityHoverInfo : MonoBehaviour
{
    public EntityGameObject EntityGameObject
    {
        get { return entityGameObject; }
        set
        {
            entityGameObject = value;
            entityGameObjectCollider = entityGameObject.gameObject.GetComponent<Collider>();
        }
    }
    private EntityGameObject entityGameObject;
    private Collider entityGameObjectCollider;
    public TextMeshProUGUI TextMeshProUGUI;

    void Update()
    {
        if (EntityGameObject)
        {
            if (PlaybackManager.Instance.PlaybackSessionCount > 1)
            {
                TextMeshProUGUI.text = entityGameObject.Entity.name + " (" + entityGameObject.PlaybackSession.LoadedSession.Session.name + ")";
            }
            else
            {
                TextMeshProUGUI.text = entityGameObject.Entity.name;
            }
            if (entityGameObjectCollider)
            {
                transform.position = new Vector3(entityGameObjectCollider.bounds.center.x, entityGameObjectCollider.bounds.max.y + 0.1f, entityGameObjectCollider.bounds.center.z);
            }
            else
            {
                transform.position = EntityGameObject.gameObject.transform.position;
            }
        }
        // transform.forward = XRCamera.Instance.transform.forward;
        transform.forward = Vector3.Normalize(XRCamera.Instance.transform.position - transform.position) * -1f;
        float distanceScale = 0.00075f * Vector3.Distance(transform.position, XRCamera.Instance.transform.position);
        transform.localScale = new Vector3(distanceScale, distanceScale, 1f);
    }
}
