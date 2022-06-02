using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MovableUIElement : Toggle, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [System.Serializable]
    public class MovedEvent : UnityEvent<GameObject>
    {
    }

    private Vector2 lastMousePosition;


    // TODO need a large invisible plane to perform ScreenPointToLocalPointInRectangle for better dragging
    public RectTransform DragPlane;
    public MovedEvent WasMoved;
    public MovedEvent IsMoved;
    public MovedEvent IsSelected;
    public float MaxClamp = 10;

    private bool isDragging;

    protected override void Start()
    {
        base.Start();
        onValueChanged.AddListener(OnSelectDeselect);
    }

    public void OnSelectDeselect(bool test)
    {
        // Debug.Log("Select at Frame: " + Time.frameCount);
        if (isDragging)
        {
            // Sometimes, Select Event is called instead of EndDrag. Hence, perform End Drag steps if select is called while dragging
            isDragging = false;
            lastMousePosition = Vector2.zero;
            WasMoved.Invoke(gameObject);
        }
        else
        {
            IsSelected.Invoke(gameObject);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdatePosition(eventData);
    }

    void UpdatePosition(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(DragPlane, eventData.position,
            eventData.pressEventCamera, out localCursor))
        {
            return;
        }

        if (lastMousePosition.x != 0)
        {
            RectTransform rect = GetComponent<RectTransform>();
            Vector2 diff = localCursor - lastMousePosition;

            float newPosition = rect.anchoredPosition.x + diff.x;
            newPosition = Mathf.Clamp(newPosition, 0,
                transform.parent.GetComponent<RectTransform>().rect.width - MaxClamp);


            rect.anchoredPosition = new Vector2(newPosition, rect.anchoredPosition.y);
            IsMoved.Invoke(gameObject);
        }

        lastMousePosition = localCursor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        //Debug.Log("Begin Drag at Frame: " + Time.frameCount);
    }

    /// <summary>
    /// This method will be called at the end of mouse drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("End Drag at Frame: " + Time.frameCount);
        isDragging = false;
        lastMousePosition = Vector2.zero;
        WasMoved.Invoke(gameObject);
    }
}