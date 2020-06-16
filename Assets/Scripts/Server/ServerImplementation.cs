using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using Random = UnityEngine.Random;
// ReSharper disable IteratorNeverReturns

[Preserve]
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
        m_state.phase = GamePhase.Playing;
        m_state.clients = m_clients.Select(c => c.ToClientData()).ToList();

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
        CardsServer.Instance.SendAllExcept(idx, MessageType.CmdOnClientDisconnect, m_clients[idx].ToClientData());
        if (m_state.currentCzar == m_clients.Count - 1) m_state.currentCzar = idx; // Swap back changes last index to the index of the player being removed.

        m_clients.RemoveAtSwapBack(idx);
        UpdateClientIds();

        m_state.clients = m_clients.Select(c => c.ToClientData()).ToList();

        foreach (var client in m_clients)
        {
            client.SyncGameState(m_state);
        }

        AdvanceToVotingIfDone();
    }

    void OnClientConnected(int idx)
    {
        var cli = new Client(idx);
        m_clients.Insert(idx, cli);
        UpdateClientIds();
        m_state.clients = m_clients.Select(c => c.ToClientData()).ToList();
        cli.SendId();
        cli.FillHand(10);

        foreach (var client in m_clients)
        {
            client.SyncGameState(m_state);
        }
    }

    void UpdateClientIds()
    {
        for (int i = 0; i < m_clients.Count; i++)
        {
            m_clients[i].id = i;
        }
    }

    [Message(MessageType.RpcSetName)]
    void OnClientSetName(ClientName name, int clientIdx)
    {
        m_clients[clientIdx].name = name.Name;
        m_state.clients = m_clients.Select(c => c.ToClientData()).ToList();
        foreach (var cli in m_clients) cli.SyncGameState(m_state);
    }

    [Message(MessageType.RpcVoteOnClient)]
    void OnClientVoted(ClientIdentifier client, int clientIdx)
    {
        if (m_state.currentCzar != clientIdx || m_state.phase != GamePhase.Voting) return;

        m_clients.Find(c => c.guid == client.Guid).score++;

        m_state.currentCzar++;
        while (m_state.currentCzar >= m_clients.Count) m_state.currentCzar -= m_clients.Count;

        foreach (var cli in m_clients)
        {
            cli.currentCards.Clear();
            cli.FillHand(10);
        }

        m_state.currentBlackCard = GetRandomCard(false);
        m_state.phase = GamePhase.Playing;
        m_state.clients = m_clients.Select(c => c.ToClientData()).ToList();
        foreach (var cli in m_clients) cli.SyncGameState(m_state);
        CardsServer.Instance.SendAll(MessageType.CmdBeginNewRound, new EmptyData());
    }

    [Message(MessageType.RpcPlayCard)]
    void ClientPlayedCard(CardDefinition card, int clientIdx)
    {
        var client = m_clients[clientIdx];
#if !UNITY_EDITOR
        if (clientIdx == m_state.currentCzar)
        {
            Debug.LogError("SRV: Cannot play card, EISCZAR");
            return;
        }
#endif
        if (!client.hand.Remove(card))
        {
            Debug.LogError("SRV: Cannot play card, ENOTINHAND");
            return;
        }

        if (client.currentCards.Count >= m_state.currentBlackCard.Pick)
        {
            Debug.LogError("SRV: Cannot play card, ELIMITREACHED");
            return;
        }

        if (m_state.phase != GamePhase.Playing)
        {
            Debug.LogError("SRV: Cannot play card, EWRONGPHASE");
            return;
        }

        client.currentCards.Add(card);
        OnCardPlayed?.Invoke(client, card);

        CardsServer.Instance.SendAll(MessageType.CmdOnClientPlayedCard, new ClientIdentifier(client.guid));
        CardsServer.Instance.Send(clientIdx, MessageType.CmdRemoveCard, card);

        AdvanceToVotingIfDone();
    }

    [Message(MessageType.RpcRevealCard)]
    void RevealCard(RevealCardData data, int clientIdx)
    {
        if (m_state.currentCzar != clientIdx || data.cardIndex >= m_state.currentBlackCard.Pick || m_state.phase != GamePhase.Voting) return;

        CardsServer.Instance.SendAll(MessageType.CmdRevealCard,
            new RevealCardData
            {
                cardIndex = data.cardIndex, 
                clientGuid = data.clientGuid, 
                hasCardDefinition = true,
                cardData = m_clients.Find(c => c.guid == data.clientGuid).currentCards[data.cardIndex]
            });
    }

    void AdvanceToVotingIfDone()
    {
        var everyoneDone = m_clients.All(cli => cli.currentCards.Count >= m_state.currentBlackCard.Pick || cli.guid == m_state.CurrentCzarGuid);

        if (everyoneDone)
        {
            m_state.phase = GamePhase.Voting;
            m_state.clients = m_clients.Select(c => c.ToClientData()).ToList();
            foreach (var client in m_clients) client.SyncGameState(m_state);
            CardsServer.Instance.SendAll(MessageType.CmdBeginVoting, new EmptyData());
        }
    }

    public CardDefinition GetRandomCard(bool white)
    {
        return (white ? m_cards.whiteCards :  m_cards.blackCards).RandomItem();
    }

    public int GenerateGuid()
    {
        int guid;
        do
        {
            guid = Random.Range(int.MinValue, int.MaxValue);
        } while (m_clients.Any(c => c.guid == guid) && m_clients.Count > 0);

        return guid;
    }
}