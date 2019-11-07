using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;

public class ServerImplementation : EventImplementor
{
    readonly List<ClientData> m_clients = new List<ClientData>();

    CardCollection m_cards;

    public static event Action<ClientData, CardDefinition> OnCardPlayed; 

    void Awake()
    {
        AttachEvent<CardsServer>(nameof(CardsServer.OnDataReceived));
        CardsServer.OnConnected += idx => m_clients.Insert(idx, new ClientData(idx));
        CardsServer.OnDisconnected += idx => m_clients.RemoveAtSwapBack(idx);

        m_cards = JsonConvert.DeserializeObject<CardCollection>(Resources.Load<TextAsset>("cards").text);
        Debug.Log(m_cards.BlackCards[0]);
        Debug.Log(m_cards.WhiteCards[0]);
    }

    [Message(MessageType.RpcPlayCard)]
    void Handle(CardDefinition card, int clientIdx)
    {
        var client = m_clients[clientIdx];

        if (client.hand.Remove(card))
        {
            OnCardPlayed?.Invoke(client, card);
        }
    }
}

class CardCollection
{
    [JsonProperty(PropertyName = "whiteCards")] public List<CardDefinition> WhiteCards;
    [JsonProperty(PropertyName = "blackCards")] public List<CardDefinition> BlackCards;
}

public class ClientData
{
    public int id;
    public List<CardDefinition> hand = new List<CardDefinition>();

    public ClientData(int id) => this.id = id;
}