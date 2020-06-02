using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fasterflect;
using UnityEngine;
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

public class ClientImplementation : EventImplementor
{
    public static ClientImplementation Instance { get; private set; }

    public static event Action<CardDefinition> OnDrawCard;
    public static event Action<GameState, GameState> OnGameStateChanged;
    public static event Action<int> OnOtherClientDisconnect;
    public static event Action<int> OnClientPlayedCard;
    public static event Action<CardDefinition> OnShouldRemoveCard;
    public static event Action OnVotingBegin;

    [SerializeField]
    int m_guid;
    public int MyGuid => m_guid;

    GameState m_lastState;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        AttachEvent<CardsClient>(nameof(CardsClient.OnDataReceived));
        CardDrag.OnCardPlayed += OnCardPlayed;
    }

    static void OnCardPlayed(Card c, Transform cTrans)
    {
        CardsClient.Instance.Send(MessageType.RpcPlayCard, c.CardDef);
    }

    [Message(MessageType.CmdRemoveCard)]
    void RemoveCard(CardDefinition card)
    {
        OnShouldRemoveCard?.Invoke(card);
    }

    [Message(MessageType.CmdSetGuid)]
    void SetId(ClientIdentifier id)
    {
        m_guid = id.Guid;
    }

    [Message(MessageType.CmdOnClientDisconnect)]
    void ClientDisconnected(ClientIdentifier identifier)
    {
        OnOtherClientDisconnect?.Invoke(identifier.Guid);
    }

    [Message(MessageType.CmdOnClientPlayedCard)]
    void ClientPlayedCard(ClientIdentifier identifier)
    {
        OnClientPlayedCard?.Invoke(identifier.Guid);
    }

    [Message(MessageType.CmdSyncGameState)]
    void SyncGameState(GameState state)
    {
        OnGameStateChanged?.Invoke(m_lastState, state);
        m_lastState = state;
    }

    [Message(MessageType.CmdBeginVoting)]
    void BeginVoting()
    {
        OnVotingBegin?.Invoke();
    }

    [Message(MessageType.CmdDrawCard)]
    void DrawCard(CardDefinition card)
    {
        OnDrawCard?.Invoke(card);
    }

    [Message(MessageType.CmdHeartbeat)]
    void Heartbeat()
    {
        CardsClient.Instance.Send(MessageType.RpcHeartbeatAck, new EmptyData(), false);
    }
}
