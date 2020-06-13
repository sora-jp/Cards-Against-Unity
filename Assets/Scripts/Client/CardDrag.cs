using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Card))]
public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static event Action<Card, Transform> OnCardPlayed;

    Vector3 offset;
    Canvas rootCanvas;
    int siblingIdx;
    Transform lastParent;
    RectTransform cardPlayArea;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        cardPlayArea = FindObjectOfType<CardPlayArea>().GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - rootCanvas.worldCamera.ScreenToWorldPoint(eventData.position);
        
        siblingIdx = transform.GetSiblingIndex();
        lastParent = transform.parent;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = rootCanvas.worldCamera.ScreenToWorldPoint(eventData.position) + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(lastParent, true);
        transform.SetSiblingIndex(siblingIdx);
        if (RectTransformUtility.RectangleContainsScreenPoint(cardPlayArea, transform.position))
        {
            OnCardPlayed?.Invoke(GetComponent<Card>(), GetComponent<RectTransform>());
        }
    }
}
