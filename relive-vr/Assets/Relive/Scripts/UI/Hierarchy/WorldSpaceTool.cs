using System.Collections.Generic;
using Relive.Tools;
using Relive.UI.Animations;
using Relive.UI.Hierarchy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Hierarchy
{
    public class WorldSpaceTool : MonoBehaviour
    {
        private HierarchyToolItem hierarchyToolItem;
        private Tool tool;
        public TextMeshProUGUI ToolName;
        public TextMeshProUGUI ResultText;
        public Image ToolImage;
        public Image ToolColorImage;
        public GameObject Slots;
        public ToolEntityItem SlotPrefab;
        public Sprite PinActiveSprite;
        public Sprite PinInActiveSprite;
        public Image PinImage;
        public PropertyWindow PropertyWindow;

        private List<ToolEntityItem> toolSlotItems;
        private BoxCollider boxCollider;
        private SlideX slideX;

        public bool isMovable;
        public bool isSelected;

        private void Start()
        {
            boxCollider = GetComponent<BoxCollider>();
            slideX = GetComponent<SlideX>();
            isMovable = true;
        }


        public HierarchyToolItem HierarchyToolItem
        {
            get { return hierarchyToolItem; }
            set
            {
                hierarchyToolItem = value;
                tool = hierarchyToolItem.Tool;
                ToolName.text = tool.Name;
                ToolImage.sprite = tool.Image;

                // XX TODO XX
                //ToolColorImage.color = tool.Color;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (tool != null)
            {
                // Fill result field

                // XX TODO XX 
                ResultText.text = "TODO";
                ToolColorImage.color = Color.red;
                //ResultText.text = tool.GetResult();
                //ToolColorImage.color = tool.Color;

                // Create slots for entities
                if (toolSlotItems == null && tool.MinEntities > 0)
                {
                    toolSlotItems = new List<ToolEntityItem>();
                    for (int i = 0; i < tool.MinEntities; i++)
                    {
                        ToolEntityItem toolSlotItem = Instantiate(SlotPrefab, Slots.transform);
                        toolSlotItems.Add(toolSlotItem);
                    }


                    // Fill slots
                    for (int i = 0; i < toolSlotItems.Count; i++)
                    {
                        if (tool.Entities[i] != null)
                        {
                            toolSlotItems[i].Name = tool.Entities[i];
                        }
                        else
                        {
                            toolSlotItems[i].Name = null;
                        }

                        LayoutRebuilder.ForceRebuildLayoutImmediate(toolSlotItems[i].GetComponent<RectTransform>());
                    }
                }
            }
        }

        public void OnCloseButtonClicked()
        {
            hierarchyToolItem.WorldSpaceTool = null;
            Destroy(gameObject);
        }

        private void SlideAnimation(bool slideOut)
        {
            if (slideOut)
                slideX.Activate();
            else
                slideX.Deactivate();
        }

        public void OnToolClicked()
        {
            isSelected = !isSelected;
            if (PropertyWindow.gameObject.activeSelf)
                PropertyWindow.gameObject.SetActive(false);
            SlideAnimation(isSelected);
        }

        public void OnPinButtonClicked()
        {
            isMovable = !isMovable;
            if (!isMovable)
            {
                boxCollider.enabled = false;
                PinImage.sprite = PinInActiveSprite;
            }
            else
            {
                boxCollider.enabled = true;
                PinImage.sprite = PinActiveSprite;
            }

            OnToolClicked();
        }

        public void OnPropertyButtonClicked()
        {
            OnToolClicked();
            PropertyWindow.gameObject.SetActive(true);
            PropertyWindow.SetParameterWindow(hierarchyToolItem);
        }
    }
}