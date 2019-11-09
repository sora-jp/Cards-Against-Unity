using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fasterflect;
using UnityEngine;

public class ClientImplementation : EventImplementor
{
    public static ClientImplementation Instance { get; private set; }

    public static event Action<CardDefinition> OnDrawCard;
    public static event Action<GameState, GameState> OnGameStateChanged;
    public static event Action<int> OnOtherClientDisconnect;
    public static event Action<int> OnClientPlayedCard;

    public int MyGuid { get; private set; }

    GameState _lastState;

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

    void OnCardPlayed(Card c, Transform cTrans)
    {
        CardsClient.Instance.Send(MessageType.RpcPlayCard, c.CardDef);
    }

    [Message(MessageType.CmdSetGuid)]
    void SetId(ClientData id)
    {
        MyGuid = id.Guid;
    }

    [Message(MessageType.CmdOnClientDisconnect)]
    void ClientDisconnected(ClientData data)
    {
        OnOtherClientDisconnect?.Invoke(data.Guid);
    }

    [Message(MessageType.CmdOnClientPlayedCard)]
    void ClientPlayedCard(ClientData data)
    {
        OnClientPlayedCard?.Invoke(data.Guid);
    }

    [Message(MessageType.CmdSyncGameState)]
    void SyncGameState(GameState state)
    {
        OnGameStateChanged?.Invoke(_lastState, state);
        _lastState = state;
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
