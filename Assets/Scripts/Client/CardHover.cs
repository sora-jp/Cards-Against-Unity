using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler
{
    public float maxYOffset;
    Canvas _canvas;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = 99;
        GetComponentInParent<CardLayout>()?.SetTargetY(transform, maxYOffset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _canvas.overrideSorting = false;
        _canvas.sortingOrder = 0;
        GetComponentInParent<CardLayout>()?.SetTargetY(transform, 0);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnPointerExit(eventData);
    }
}
