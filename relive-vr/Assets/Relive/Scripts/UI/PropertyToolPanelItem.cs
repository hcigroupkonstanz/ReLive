using UnityEngine;
using TMPro;
using Relive.Data;

public class PropertyToolPanelItem : MonoBehaviour
{
    public TextMeshProUGUI PropertyText;
    public EntityGameObject EntityGameObject;
    public string VariableName;

    void Update()
    {
        if (EntityGameObject && VariableName != "")
        {
            var data = EntityGameObject.GetData(VariableName);
            if (data != null)
                PropertyText.text = VariableName + ": " + data;
            else
                PropertyText.text = VariableName + ": " + "not available";
        }
    }
}