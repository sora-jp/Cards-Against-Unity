using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlackCardUiManager : MonoBehaviour
{
    public Card blackCard;
    public SlideFromEdge blackCardSlide;

    void Awake()
    {
        ClientImplementation.OnGameStateChanged += OnGameStateChange;
    }

    void OnDestroy()
    {
        ClientImplementation.OnGameStateChanged -= OnGameStateChange;
    }

    private void OnGameStateChange(GameState oldState, GameState newState)
    {
        if (oldState != null && oldState.currentBlackCard != newState.currentBlackCard)
        {
            DOTween.Sequence()
                .Append(blackCard.transform.DOLocalRotate(new Vector3(0, 180, 0), 0.75f).SetEase(Ease.InOutCubic))
                .Append(DOTween.To(v => blackCardSlide.transition = v, 1, 0, 0.5f).SetEase(Ease.InOutCubic))
                .AppendCallback(() => blackCard.CardDef = newState.currentBlackCard)
                .AppendInterval(0.5f)
                .Append(DOTween.To(v => blackCardSlide.transition = v, 0, 1, 0.5f).SetEase(Ease.InOutCubic))
                .Append(blackCard.transform.DOLocalRotate(Vector3.zero, 0.75f).SetEase(Ease.InOutCubic));
        }
        else if (oldState == null)
        {
            blackCard.CardDef = newState.currentBlackCard;
        }
    }
}
