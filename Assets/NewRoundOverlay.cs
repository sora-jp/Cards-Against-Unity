using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class NewRoundOverlay : MonoBehaviour
{
    CanvasGroup m_group;

    void Awake()
    {
        m_group = GetComponent<CanvasGroup>();
        ClientImplementation.OnGameStateChanged += FirstStateChange;
        ClientImplementation.OnNewRoundBegin += NewRoundBegin;
    }

    void NewRoundBegin()
    {
        ClientImplementation.OnNewRoundBegin -= NewRoundBegin;
        m_group.blocksRaycasts = m_group.interactable = false;
        m_group.DOFade(0, 0.5f).SetEase(Ease.InOutCubic);
    }

    void FirstStateChange(GameState arg1, GameState arg2)
    {
        ClientImplementation.OnGameStateChanged -= FirstStateChange;

        if (arg2.phase != GamePhase.Playing)
        {
            m_group.alpha = 1;
            m_group.blocksRaycasts = true;
            m_group.interactable = true;
        }
    }
}
