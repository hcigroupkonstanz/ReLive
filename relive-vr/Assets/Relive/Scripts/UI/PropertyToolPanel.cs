using System.Collections;
using System.Collections.Generic;
using Relive.Data;
using UnityEngine;

public class PropertyToolPanel : MonoBehaviour
{
    Dictionary<string, PropertyToolPanelItem> propertyToolPanelItems = new Dictionary<string, PropertyToolPanelItem>();

    public GameObject Content;

    public PropertyToolPanelItem propertyToolPanelItemPrefab;

    public EntityGameObject entityGameObject;


    void Update()
    {
        this.transform.LookAt(Camera.main.transform);
        if (entityGameObject)
        {
            Collider collider = entityGameObject.gameObject.GetComponent<Collider>();
            if (!collider) collider = entityGameObject.gameObject.GetComponentInChildren<Collider>();
            this.transform.position = new Vector3(entityGameObject.gameObject.transform.position.x,
                collider.bounds.max.y + 0.2f, entityGameObject.gameObject.transform.position.z);
        }
    }

    public PropertyToolPanelItem AddPropertyToolPanelItem(string variableName)
    {
        if (propertyToolPanelItems.ContainsKey(variableName))
            return propertyToolPanelItems[variableName];

        PropertyToolPanelItem item = Instantiate(propertyToolPanelItemPrefab.gameObject, Content.transform)
            .GetComponent<PropertyToolPanelItem>();
        item.EntityGameObject = entityGameObject;
        item.VariableName = variableName;
        propertyToolPanelItems.Add(variableName, item);
        return item;
    }

    public void DestroyPropertyToolPanelItem(string Name)
    {
        if (!propertyToolPanelItems.ContainsKey(Name))
            return;

        PropertyToolPanelItem item = propertyToolPanelItems[Name];
        propertyToolPanelItems.Remove(Name);
        Destroy(item.gameObject);
        /*if (propertyToolPanelItems.Count == 0)
        {
            Destroy(this.gameObject);
        }*/
    }
}