using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardContainerForPlayer : MonoBehaviour
{
    public ClientData linkedClient;
    public int spacingAfterExpand;

    VerticalLayoutGroup m_group;
    [HideInInspector, NonSerialized] public bool isExpanded;

    void Awake()
    {
        m_group = GetComponent<VerticalLayoutGroup>();
        ClientImplementation.OnVotingBegin += OnVotingBegin;
    }

    void OnDestroy()
    {
        ClientImplementation.OnVotingBegin -= OnVotingBegin;
    }

    void OnVotingBegin()
    {
        Expand();
    }

    public void Expand()
    {
        isExpanded = true;
        m_group.spacing = spacingAfterExpand;
    }
}
