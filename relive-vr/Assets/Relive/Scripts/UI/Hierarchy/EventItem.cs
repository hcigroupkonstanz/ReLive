using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Relive.Data;
using Relive.UI.Hierarchy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventItem : MonoBehaviour
{
    public EventGameObject EventGameObject
    {
        get { return eventGameObject; }
        set
        {
            eventGameObject = value;
            EventName.text = eventGameObject.Event.name;
            // VisibleToggle.SetIsOnWithoutNotify(!eventGameObject.Hide);
            ToolCountString.text = "0 Active Tools";
            eventGameObject.OnVisibleChanged += OnVisibleChanged;
            eventGameObject.OnSelectedChanged += OnSelectionChanged;
            isEventSet = true;
        }
    }
    public GameObject SelectedIcon;
    public TextMeshProUGUI EventName;
    public TextMeshProUGUI ToolCountString;
    public Toggle VisibleToggle;
    public Sprite OnSprite;
    public Sprite OffSprite;
    public Image StatusImage;
    public Image ToolActiveImage;
    public Image Background;
    public Image VisibleToggleImage;

    private EventGameObject eventGameObject;
    private bool isSelectionToogleSet;
    private bool isEventSet = false;

    void Update()
    {
        if (!eventGameObject && isEventSet)
            Destroy(gameObject);
        // ToolCountString.text = ActiveToolPanelVR.Instance.Tools.Count(t => t.Entities.Contains(entityGameObject.Entity.name)) + " Active Tools";
        VisibleToggle.SetIsOnWithoutNotify(!eventGameObject.Hide);
        if (!eventGameObject.Hide)
            StatusImage.sprite = OnSprite;
        else
            StatusImage.sprite = OffSprite;
    }


    public void OnVisibleToggleChanged(bool state)
    {
        eventGameObject.Hide = !state;

        if (state)
            StatusImage.sprite = OnSprite;
        else
            StatusImage.sprite = OffSprite;
    }

    public void OnSelectionToggleSet()
    {
        eventGameObject.ChangeSelected();
    }

    public void OnSelectionChanged(bool selected)
    {
        Color currentColor = Background.color;
        if (selected)
        {
            // Switch the background sprite on selection, because the default UI Sprite renders more greyly.
            //Sprite activeSprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            //Background.sprite = activeSprite;
            //ToolActiveImage.sprite = activeSprite;
            //VisibleToggleImage.sprite = activeSprite;
            Background.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
            SelectedIcon.SetActive(true);
        }
        else
        {
            //Sprite activeSprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            //Background.sprite = activeSprite;
            //ToolActiveImage.sprite = activeSprite;
            //VisibleToggleImage.sprite = activeSprite;
            Background.color = new Color(currentColor.r, currentColor.g, currentColor.b, 150f / 255f);
            SelectedIcon.SetActive(false);
        }
    }


    private void OnVisibleChanged(bool visible)
    {
        eventGameObject.Visible = visible;
        if (visible)
        {
            // change alpha of status image
            Color currentColor = StatusImage.color;
            StatusImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
            // change text alpha
            currentColor = EventName.color;
            EventName.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
            ToolCountString.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
            // change alpha of tools active rectangle
            currentColor = ToolActiveImage.color;
            ToolActiveImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
            VisibleToggleImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
        }
        else
        {
            float alphaValue = 150f / 255f;
            // change alpha of status image
            Color currentColor = StatusImage.color;
            StatusImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
            // change text alpha
            currentColor = EventName.color;
            EventName.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
            ToolCountString.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
            // change alpha of tools active rectangle
            currentColor = ToolActiveImage.color;
            ToolActiveImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
            VisibleToggleImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
        }
    }
}
