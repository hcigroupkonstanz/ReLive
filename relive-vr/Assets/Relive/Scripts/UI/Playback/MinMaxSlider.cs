using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MinMaxSlider : MonoBehaviour
{
    public MovableUIElement incidentRange;

    public Slider MinSlider;

    public Slider MaxSlider;

    private RectTransform RectTransform;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        MinSlider.onValueChanged.AddListener(OnMinSliderValueChanged);
        MaxSlider.onValueChanged.AddListener(OnMaxSliderValueChanged);
        incidentRange.IsMoved.AddListener(RangeIsMoved);
    }

    private void RangeIsMoved(GameObject movedObject)
    {
        RectTransform movedObjectRectTransform = movedObject.GetComponent<RectTransform>();
        float anchoredXPosition = movedObjectRectTransform.anchoredPosition.x;
        float xStartPosNew = anchoredXPosition;
        float xEndPosNew = anchoredXPosition + movedObjectRectTransform.rect.width;

        var rect = RectTransform.rect;
        MinSlider.value = xStartPosNew / rect.width * MaxSlider.maxValue;
        MaxSlider.value = xEndPosNew / rect.width * MaxSlider.maxValue;
    }

    private void OnMinSliderValueChanged(float value)
    {
        MinSlider.value = Mathf.Clamp(MinSlider.value, MinSlider.minValue, MaxSlider.value);
        UpdateRangeBar();
    }
    
    private void OnMaxSliderValueChanged(float value)
    {
        MaxSlider.value = Mathf.Clamp(MaxSlider.value, MinSlider.value, MaxSlider.maxValue);
        UpdateRangeBar();
    }

    private void Update()
    {
        UpdateRangeBar();
    }

    void UpdateRangeBar()
    {
        float normalizedMaxSliderValue = MaxSlider.value / MaxSlider.maxValue;
        float normalizedMinSliderValue = MinSlider.value / MaxSlider.maxValue;
        float normalizedWidth = normalizedMaxSliderValue - normalizedMinSliderValue;
        float parentWidth = RectTransform.rect.width;
        float width = normalizedWidth * parentWidth;
        float xPos = normalizedMinSliderValue * parentWidth;
        RectTransform incidentRect = incidentRange.GetComponent<RectTransform>();
        incidentRect.anchoredPosition = new Vector2(xPos, incidentRect.anchoredPosition.y);
        incidentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        incidentRange.MaxClamp = width;
    }

    public void SetNormalizedMinMaxValues(float normalizedXStart, float normalizedXEnd)
    {
        MinSlider.normalizedValue = normalizedXStart;
        MaxSlider.normalizedValue = normalizedXEnd;
        UpdateRangeBar();
    }
}
