using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

public class ClientImplementation : EventImplementor
{
    public static ClientImplementation Instance { get; private set; }

    public static event Action<CardDefinition> OnDrawCard;
    public static event Action<GameState, GameState> OnGameStateChanged;
    public static event Action<ClientData> OnOtherClientDisconnected;
    public static event Action<ClientData> OnOtherClientConnected;
    public static event Action<int> OnClientPlayedCard;
    public static event Action<CardDefinition> OnShouldRemoveCard;
    public static event Action<RevealCardData> OnRevealCard;
    public static event Action OnVotingBegin;
    public static event Action OnNewRoundBegin;

    [SerializeField]
    int m_guid;
    public int MyGuid => m_guid;

    public GameState State { get; private set; }

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
        PlayedCard.OnCardRevealRequested += OnRevealRequested;
        ClientVoteButton.OnVoteForClient += OnShouldVoteForClient;
    }

    static void OnShouldVoteForClient(int playerGuid)
    {
        CardsClient.Instance.Send(MessageType.RpcVoteOnClient, new ClientIdentifier(playerGuid));
    }

    static void OnRevealRequested(int playerGuid, int cardIdx)
    {
        CardsClient.Instance.Send(MessageType.RpcRevealCard, new RevealCardData {cardIndex = cardIdx, clientGuid = playerGuid});
    }

    static void OnCardPlayed(Card c, Transform cTrans)
    {
        CardsClient.Instance.Send(MessageType.RpcPlayCard, c.CardDef);
    }

    public void SetName(string newName)
    {
        CardsClient.Instance.Send(MessageType.RpcSetName, new ClientName {Name = newName});
    }

    [Message(MessageType.CmdRevealCard)]
    void RevealCard(RevealCardData data)
    {
        OnRevealCard?.Invoke(data);
    }

    [Message(MessageType.CmdRemoveCard)]
    void RemoveCard(CardDefinition card)
    {
        OnShouldRemoveCard?.Invoke(card);
    }

    [Message(MessageType.CmdSetGuid)]
    void SetId(ClientIdentifier id)
    {
        m_guid = id.Guid;
    }

    [Message(MessageType.CmdSyncGameState)]
    void SyncGameState(GameState state)
    {
        OnGameStateChanged?.Invoke(State, state);
        var last = State;
        var cur = state;
        State = state;

        switch (cur.phase)
        {
            case GamePhase.Voting when last?.phase != GamePhase.Voting:
                OnVotingBegin?.Invoke();
                break;
            case GamePhase.Playing when last?.phase != GamePhase.Playing:
                OnNewRoundBegin?.Invoke();
                break;
            case GamePhase.Waiting when last?.phase != GamePhase.Waiting:
            default:
                break;
        }

        var clientsConnected = cur.clients.Except(last?.clients ?? Enumerable.Empty<ClientData>());
        var clientsDisconnected = last?.clients.Except(cur.clients) ?? Enumerable.Empty<ClientData>();

        foreach (var client in clientsDisconnected) OnOtherClientDisconnected?.Invoke(client);
        foreach (var client in clientsConnected) OnOtherClientConnected?.Invoke(client);

        foreach (var client in cur.clients)
        {
            int cardsPlayed;
            if (last == null) cardsPlayed = client.currentCardAmt;
            else if (!last.clients.Contains(client)) cardsPlayed = 0;
            else cardsPlayed = client.currentCardAmt - last.clients.Single(c => c.guid == client.guid).currentCardAmt;

            for (var i = 0; i < cardsPlayed; i++)
            {
                OnClientPlayedCard?.Invoke(client.guid);
            }
        }
    }

    [Message(MessageType.CmdDrawCard)]
    void DrawCard(CardDefinition card)
    {
        OnDrawCard?.Invoke(card);
    }

    [Message(MessageType.Heartbeat)]
    void Heartbeat()
    {
        CardsClient.Instance.Send(MessageType.Heartbeat, new EmptyData(), false);
    }

    public ClientData ClientFromGuid(int cliGuid)
    {
        return State?.clients.Single(c => c.guid == cliGuid);
    }
}
