using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ServerImplementation : EventImplementor
{
    public static ServerImplementation Instance { get; private set; }

    [SerializeField]
    List<Client> m_clients = new List<Client>();

    [SerializeField]
    CardCollection m_cards;

    [SerializeField]
    GameState m_state = new GameState();

    public static event Action<Client, CardDefinition> OnCardPlayed; 

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        Random.InitState(DateTime.Now.Millisecond);

        AttachEvent<CardsServer>(nameof(CardsServer.OnDataReceived));
        CardsServer.OnConnected += OnClientConnected;
        CardsServer.OnDisconnected += OnClientDisconnect;

        m_cards = JsonConvert.DeserializeObject<CardCollection>(Resources.Load<TextAsset>("cards").text);
        m_state.currentBlackCard = GetRandomCard(false);

        StartCoroutine(_heartbeat());
    }

    void OnClientDisconnect(int idx)
    {
        CardsServer.Instance.SendAllExcept(idx, MessageType.CmdOnClientDisconnect, new ClientData(m_clients[idx].guid));
        m_clients.RemoveAtSwapBack(idx);
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
        cli.SendId();
        cli.SyncGameState(m_state);
        cli.DrawCards(10);
    }

    [Message(MessageType.RpcPlayCard)]
    void ClientPlayedCard(CardDefinition card, int clientIdx)
    {
        var client = m_clients[clientIdx];
        if (!client.hand.Remove(card)) return;
        
        client.currentCards.Add(card);
        OnCardPlayed?.Invoke(client, card);

        CardsServer.Instance.SendAllExcept(clientIdx, MessageType.CmdOnClientPlayedCard, new ClientData(client.guid));

        var everyoneDone = m_clients.All(cli => cli.currentCards.Count >= m_state.currentBlackCard.Pick);

        if (everyoneDone)
        {
            CardsServer.Instance.SendAll(MessageType.CmdBeginVoting, new EmptyData());
        }
    }

    public CardDefinition GetRandomCard(bool white)
    {
        return (white ? m_cards.WhiteCards :  m_cards.BlackCards).RandomItem();
    }
}

internal static class ListE
{
    public static T RandomItem<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}

[Serializable]
internal class CardCollection
{
    [JsonProperty(PropertyName = "whiteCards")] public List<CardDefinition> WhiteCards;
    [JsonProperty(PropertyName = "blackCards")] public List<CardDefinition> BlackCards;
}

[Serializable]
public class Client
{
    public int id;
    public int guid;
    public List<CardDefinition> hand = new List<CardDefinition>();
    public List<CardDefinition> currentCards = new List<CardDefinition>();

    public Client(int id)
    {
        this.id = id;
        guid = Random.Range(0, int.MaxValue);
    }

    public void DrawCards(int amt)
    {
        for (int i = 0; i < amt; i++) DrawCard();
    }

    public void DrawCard()
    {
        var card = ServerImplementation.Instance.GetRandomCard(true);
        CardsServer.Instance.Send(id, MessageType.CmdDrawCard, card);
        hand.Add(card);
    }

    public void SyncGameState(GameState state)
    {
        CardsServer.Instance.Send(id, MessageType.CmdSyncGameState, state);
    }

    public void Heartbeat()
    {
        CardsServer.Instance.Send(id, MessageType.CmdHeartbeat, new EmptyData(), false);
    }

    public void SendId()
    {
        CardsServer.Instance.Send(id, MessageType.CmdSetGuid, new ClientData(guid));
    }
}