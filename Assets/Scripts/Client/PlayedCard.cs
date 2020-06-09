using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayedCard : MonoBehaviour, IPointerDownHandler
{
    public static event Action<int, int> OnCardRevealRequested;

    CanvasGroup m_group;
    CardContainerForPlayer m_parent;
    Card m_card;

    void Awake()
    {
        m_group = GetComponent<CanvasGroup>();
        m_group.alpha = 1;
        transform.localScale = new Vector3(0, 1, 1);
        transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.InOutQuad);
        m_card = GetComponent<Card>();
        ClientImplementation.OnRevealCard += RevealCard;
    }

    void OnDestroy()
    {
        ClientImplementation.OnRevealCard -= RevealCard;
    }

    void Start()
    {
        m_parent = GetComponentInParent<CardContainerForPlayer>();
    }

    void RevealCard(RevealCardData data)
    {
        if (data.clientGuid != m_parent.linkedClient.guid || data.cardIndex != transform.GetSiblingIndex()) return;
        m_card.CardDef = data.cardData;
        transform.DORotate(Vector3.zero, 0.75f).SetEase(Ease.InOutCubic);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!m_parent.isExpanded) m_parent.Expand();
        else OnCardRevealRequested?.Invoke(m_parent.linkedClient.guid, transform.GetSiblingIndex());
    }
}
