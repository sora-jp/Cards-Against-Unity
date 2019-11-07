using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fasterflect;
using UnityEngine;

public class ClientImplementation : EventImplementor
{
    public static event Action<CardDefinition> OnDrawCard; 

    void Awake()
    {
        AttachEvent<CardsClient>(nameof(CardsClient.OnDataReceived));
    }

    [Message(MessageType.CmdDrawCard)]
    void DrawCard(CardDefinition card)
    {
        OnDrawCard?.Invoke(card);
    }
}
