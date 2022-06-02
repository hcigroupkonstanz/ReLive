using System;
using UnityEngine;
using TMPro;
using Relive.Data;
using UnityEngine.UI;
using System.Linq;

namespace Relive.UI.Hierarchy
{
    public class HierarchyEntityItem : MonoBehaviour
    {
        public EntityGameObject EntityGameObject
        {
            get { return entityGameObject; }
            set
            {
                entityGameObject = value;
                EntityName.text = entityGameObject.Entity.name;
                ToolCountString.text = "0 Active Tools";
                entityGameObject.OnVisibleChanged += OnVisibleChanged;
                entityGameObject.OnSelectedChanged += OnSelectionChanged;
                isEntitySet = true;
            }
        }

        public GameObject SelectedIcon;
        public TextMeshProUGUI EntityName;
        public TextMeshProUGUI ToolCountString;
        public Toggle VisibleToggle;
        public Sprite OnSprite;
        public Sprite OffSprite;
        public Image StatusImage;
        public Image ToolActiveImage;
        public Image Background;
        public Image VisibleToggleImage;

        private EntityGameObject entityGameObject;
        private bool isSelectionToogleSet;
        private bool isEntitySet = false;


        void Update()
        {
            if (!entityGameObject && isEntitySet)
                Destroy(gameObject);
            ToolCountString.text = ActiveToolPanelVR.Instance.Tools.Count(t => t.Entities.Contains(entityGameObject.Entity.name)) + " Active Tools";
            VisibleToggle.SetIsOnWithoutNotify(!entityGameObject.Hide);
            if (!entityGameObject.Hide)
                StatusImage.sprite = OnSprite;
            else
                StatusImage.sprite = OffSprite;
        }


        public void OnVisibleToggleChanged(bool state)
        {
            entityGameObject.Hide = !state;

            if (state)
                StatusImage.sprite = OnSprite;
            else
                StatusImage.sprite = OffSprite;
        }

        public void OnSelectionToggleSet()
        {
            entityGameObject.ChangeSelected();
        }

        public void OnSelectionChanged(bool selected)
        {
            Color currentColor = Background.color;
            if (selected)
            {
                // Switch the background sprite on selection, because the default UI Sprite renders more greyly.
                //Sprite activeSprite = Resources.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
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
            entityGameObject.Visible = visible;
            if (visible)
            {
                // change alpha of status image
                Color currentColor = StatusImage.color;
                StatusImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
                // change text alpha
                currentColor = EntityName.color;
                EntityName.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
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
                currentColor = EntityName.color;
                EntityName.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
                ToolCountString.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
                // change alpha of tools active rectangle
                currentColor = ToolActiveImage.color;
                ToolActiveImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
                VisibleToggleImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alphaValue);
            }
        }
    }
}