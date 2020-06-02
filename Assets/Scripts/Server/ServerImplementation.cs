using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
// ReSharper disable IteratorNeverReturns

public class ServerImplementation : EventImplementor
{
    public static ServerImplementation Instance { get; private set; }

    [SerializeField] readonly List<Client> m_clients = new List<Client>();
    [SerializeField] CardCollection m_cards;
    [SerializeField] readonly GameState m_state = new GameState();

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

        m_cards = JsonConvert.DeserializeObject<CardCollection>(Resources.Load<TextAsset>("cards").text);
        m_state.currentBlackCard = GetRandomCard(false);
        m_state.SetClientSource(m_clients);

        AttachEvent<CardsServer>(nameof(CardsServer.OnDataReceived));
        CardsServer.OnConnected += OnClientConnected;
        CardsServer.OnDisconnected += OnClientDisconnect;

        StartCoroutine(_heartbeat());
    }

    IEnumerator _heartbeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            m_clients.ForEach(c => c.Heartbeat());
        }
    }

    void OnClientDisconnect(int idx)
    {
        CardsServer.Instance.SendAllExcept(idx, MessageType.CmdOnClientDisconnect, new ClientIdentifier(m_clients[idx].guid));
        m_clients.RemoveAtSwapBack(idx);
    }

    void OnClientConnected(int idx)
    {
        var cli = new Client(idx);
        m_clients.Insert(idx, cli);
        cli.SendId();
        cli.SyncGameState(m_state);
        cli.FillHand(10);
    }

    [Message(MessageType.RpcVoteOnClient)]
    void OnClientVoted(ClientIdentifier client, int clientIdx)
    {
        if (m_state.currentCardCzar != clientIdx || m_state.phase != GamePhase.Voting) return;

        m_clients.Find(c => c.guid == client.Guid).score++;

        m_state.currentCardCzar++;
        while (m_state.currentCardCzar >= m_clients.Count) m_state.currentCardCzar -= m_clients.Count;

        foreach (var cli in m_clients) cli.FillHand(10);

        CardsServer.Instance.SendAll(MessageType.CmdBeginNewRound, new EmptyData());
        foreach (var cli in m_clients) cli.SyncGameState(m_state);
    }

    [Message(MessageType.RpcPlayCard)]
    void ClientPlayedCard(CardDefinition card, int clientIdx)
    {
        var client = m_clients[clientIdx];
        if (!client.hand.Remove(card) || client.currentCards.Count >= m_state.currentBlackCard.Pick || m_state.phase != GamePhase.Playing) return;

        client.currentCards.Add(card);
        OnCardPlayed?.Invoke(client, card);

        CardsServer.Instance.SendAll(MessageType.CmdOnClientPlayedCard, new ClientIdentifier(client.guid));
        CardsServer.Instance.Send(clientIdx, MessageType.CmdRemoveCard, card);

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