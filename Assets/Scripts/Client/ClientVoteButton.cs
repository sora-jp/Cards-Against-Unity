using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ClientVoteButton : MonoBehaviour
{
    public static event Action<int> OnVoteForClient;

    public ClientData linkedClient;
    Button m_button;
    CanvasGroup m_group;
    bool m_added;

    void OnDestroy()
    {
        if (m_added) ClientImplementation.OnVotingBegin -= OnVotingBegin;
    }

    void Start()
    {
        m_group = GetComponent<CanvasGroup>();
        m_group.alpha = 0;
        m_group.blocksRaycasts = false;

        m_button = GetComponent<Button>();
        m_button.onClick.AddListener(() =>
        {
            OnVoteForClient?.Invoke(linkedClient.guid);
        });

        if (ClientImplementation.Instance.State.phase == GamePhase.Voting)
        {
            OnVotingBegin();
            return;
        }

        ClientImplementation.OnVotingBegin += OnVotingBegin;
        m_added = true;
    }

    void OnVotingBegin()
    {
        if (ClientImplementation.Instance.MyGuid != ClientImplementation.Instance.State.CurrentCzarGuid) return;
        m_group.DOFade(1, 0.5f);
        m_group.blocksRaycasts = true;
    }

    void LateUpdate()
    {
        transform.SetAsLastSibling();
    }
}
