using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Relive.UI.Hierarchy
{
    [RequireComponent(typeof(RectTransform))]
    public class HeightController : UIBehaviour
    {
        public List<RectTransform> HeightTransform;
        public List<RectTransform> YTransforms;
        public RectTransform ParentTransform;
        public int MinValue;

        // Start is called before the first frame update
        private VerticalLayoutGroup layoutGroup;
        private float lastValue;

        private void Start()
        {
            layoutGroup = GetComponent<VerticalLayoutGroup>();
            lastValue = CalculateHeightSum();
        }

        private float CalculateHeightSum()
        {
            float heightCount = 0;
            if (layoutGroup != null)
            {
                foreach (Transform child in layoutGroup.transform)
                {
                    if (child.gameObject.activeSelf)
                        heightCount += child.GetComponent<RectTransform>().rect.height + layoutGroup.spacing;
                }
            }

            return heightCount;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            float modifiedHeightSum = CalculateHeightSum();

            if (MinValue <= modifiedHeightSum || lastValue >= MinValue)
            {
                float difference = (modifiedHeightSum - lastValue);

                foreach (RectTransform rectTransform in HeightTransform)
                {
                    var sizeDelta = rectTransform.sizeDelta;
                    sizeDelta = new Vector2(sizeDelta.x,
                        sizeDelta.y + difference);
                    rectTransform.sizeDelta = sizeDelta;
                }

                foreach (RectTransform rectTransform in YTransforms)
                {
                    var position = rectTransform.localPosition;
                    position = new Vector3(position.x, position.y - difference / 2, position.z);
                    rectTransform.localPosition = position;
                }

                base.OnRectTransformDimensionsChange();
                LayoutRebuilder.ForceRebuildLayoutImmediate(ParentTransform);
            }

            lastValue = modifiedHeightSum;
        }
    }
}