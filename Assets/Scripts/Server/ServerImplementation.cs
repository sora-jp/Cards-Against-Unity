using System;
using System.Collections;
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
    readonly GameState m_state = new GameState();

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
        m_state.currentBlackCard = GetRandomCard(false);

        StartCoroutine(_heartbeat());
    }

    IEnumerator _heartbeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            m_clients.ForEach(c => c.Heartbeat());
        }
    }

    void OnClientConnected(int idx)
    {
        var cli = new Client(idx);
        m_clients.Insert(idx, cli);
        cli.SyncGameState(m_state);
        cli.DrawCards(10);
    }

    [Message(MessageType.RpcPlayCard)]
    void Handle(CardDefinition card, int clientIdx)
    {
        var client = m_clients[clientIdx];

        if (client.hand.Remove(card))
        {
            client.currentCards.Add(card);
            OnCardPlayed?.Invoke(client, card);
        }

        bool everyoneDone = true;
        foreach (var cli in m_clients)
        {
            everyoneDone &= client.currentCards.Count >= m_state.currentBlackCard.Pick;
        }

        if (everyoneDone)
        {
            
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
    public List<CardDefinition> currentCards;

    public Client(int id) => this.id = id;

    public void DrawCards(int amt)
    {
        for (int i = 0; i < amt; i++) DrawCard();
    }

    public void DrawCard()
    {
        CardsServer.Instance.Send(id, MessageType.CmdDrawCard, ServerImplementation.Instance.GetRandomCard(true));
    }

    public void SyncGameState(GameState state)
    {
        CardsServer.Instance.Send(id, MessageType.CmdSyncGameState, state);
    }

    public void Heartbeat()
    {
        CardsServer.Instance.Send(id, MessageType.CmdHeartbeat, new EmptyData());
    }
}