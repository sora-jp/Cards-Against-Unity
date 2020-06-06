using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardCzarText : MonoBehaviour
{
    SlideFromEdge m_slide;

    void Awake()
    {
        m_slide = GetComponent<SlideFromEdge>();
        ClientImplementation.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        ClientImplementation.OnGameStateChanged -= OnGameStateChanged;
    }

    void OnGameStateChanged(GameState prev, GameState cur)
    {
        float target = cur.CurrentCzarGuid == ClientImplementation.Instance.MyGuid ? 0 : 1;
        if (prev?.phase != GamePhase.Voting && cur.phase == GamePhase.Voting) target = 0;
        if (prev == null)
        {
            m_slide.transition = target;
        }
        else
        {
            if (prev.CurrentCzarGuid == cur.CurrentCzarGuid) return;
            DOTween.To(a => m_slide.transition = a, m_slide.transition, target, 0.75f).SetEase(Ease.InOutCubic);
        }
    }
}
