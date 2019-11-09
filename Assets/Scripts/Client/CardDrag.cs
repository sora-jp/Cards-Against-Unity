using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Vector3 offset;
    Canvas rootCanvas;
    int siblingIdx;
    Transform lastParent;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - (Vector3)eventData.position;
        
        siblingIdx = transform.GetSiblingIndex();
        lastParent = transform.parent;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = (Vector3)eventData.position + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(lastParent, true);
        transform.SetSiblingIndex(siblingIdx);
    }
}
