using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fasterflect;
using UnityEngine;

public class ClientImplementation : EventImplementor
{
    public static event Action<CardDefinition> OnDrawCard;
    public static event Action<GameState, GameState> OnGameStateChanged;

    GameState _lastState;

    void Awake()
    {
        AttachEvent<CardsClient>(nameof(CardsClient.OnDataReceived));
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
    void Heartbeat(EmptyData _)
    {
        CardsClient.Instance.Send(MessageType.RpcHeartbeatAck, new EmptyData());
    }
}
