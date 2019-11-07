using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ServerImplementation : EventImplementor
{
    public static ServerImplementation Instance { get; private set; }

    readonly List<Client> m_clients = new List<Client>();

    CardCollection m_cards;

    public static event Action<Client, CardDefinition> OnCardPlayed; 

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        AttachEvent<CardsServer>(nameof(CardsServer.OnDataReceived));
        CardsServer.OnConnected += OnClientConnected;
        CardsServer.OnDisconnected += idx => m_clients.RemoveAtSwapBack(idx);

        m_cards = JsonConvert.DeserializeObject<CardCollection>(Resources.Load<TextAsset>("cards").text);
    }

    void OnClientConnected(int idx)
    {
        var cli = new Client(idx);
        m_clients.Insert(idx, cli);
        cli.DrawCards(10);
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

    public CardDefinition GetRandomCard(bool white)
    {
        return (white ? m_cards.WhiteCards :  m_cards.BlackCards).RandomItem();
    }
}

static class ListE
{
    public static T RandomItem<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}

class CardCollection
{
    [JsonProperty(PropertyName = "whiteCards")] public List<CardDefinition> WhiteCards;
    [JsonProperty(PropertyName = "blackCards")] public List<CardDefinition> BlackCards;
}

public class Client
{
    public int id;
    public List<CardDefinition> hand = new List<CardDefinition>();

    public Client(int id) => this.id = id;

    public void DrawCards(int amt)
    {
        for (int i = 0; i < amt; i++) DrawCard();
    }

    public void DrawCard()
    {
        CardsServer.Instance.Send(id, MessageType.CmdDrawCard, ServerImplementation.Instance.GetRandomCard(true));
    }
}