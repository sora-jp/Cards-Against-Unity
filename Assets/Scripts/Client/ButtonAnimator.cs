using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform target;
    public float offsetHover, offsetPressed;
    public float transitionDuration;

    float m_start;

    void Awake()
    {
        m_start = target.localPosition.y;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        target.DOLocalMoveY(m_start + offsetHover, transitionDuration).SetEase(Ease.InOutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target.DOLocalMoveY(m_start, transitionDuration).SetEase(Ease.InOutQuad);
    }

    public void OnSubmit(BaseEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        target.DOLocalMoveY(m_start + offsetHover, transitionDuration).SetEase(Ease.InOutQuad);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        target.DOLocalMoveY(m_start + offsetPressed, transitionDuration).SetEase(Ease.InOutQuad);
    }
}
