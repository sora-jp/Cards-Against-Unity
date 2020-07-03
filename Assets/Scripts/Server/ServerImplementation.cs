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

    readonly List<Client> m_clients = new List<Client>();
    readonly GameState m_state = new GameState();
    CardPackList m_packs;
    CardCollection m_cards;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        Random.InitState(DateTime.Now.Millisecond);

        m_packs = JsonConvert.DeserializeObject<CardPackList>(Resources.Load<TextAsset>("cards").text);
        m_cards = m_packs.GetCollectionForPacks("Base");

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
        if (m_state.currentCzar == m_clients.Count - 1) m_state.currentCzar = idx; // Swap back changes last index to the index of the player being removed.

        m_clients.RemoveAtSwapBack(idx);
        UpdateClientIds();

        SyncGameState();

        AdvanceToVotingIfDone();
    }

    void OnClientConnected(int idx)
    {
        var cli = new Client(idx);
        m_clients.Insert(idx, cli);
        UpdateClientIds();
        cli.SendId();
        cli.FillHand(10);

        SyncGameState();
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
        SyncGameState();
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
        SyncGameState();
    }

    [Message(MessageType.RpcPlayCard)]
    void ClientPlayedCard(CardDefinition card, int clientIdx)
    {
        var client = m_clients[clientIdx];
        if (clientIdx == m_state.currentCzar)
        {
            Debug.LogError("SRV: Cannot play card, EISCZAR");
            return;
        }

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

        SyncGameState();
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
            SyncGameState();
        }
    }

    void SyncGameState()
    {
        m_state.clients = m_clients.Select(c => c.ToClientData()).ToList();
        m_clients.ForEach(c => c.SyncGameState(m_state));
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