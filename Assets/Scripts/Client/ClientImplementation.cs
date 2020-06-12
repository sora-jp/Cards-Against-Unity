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

    public void SetName(string name)
    {
        CardsClient.Instance.Send(MessageType.RpcSetName, new ClientName {Name = name});
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

    [Message(MessageType.CmdOnClientDisconnect)]
    void ClientDisconnected(ClientData data)
    {
        OnOtherClientDisconnected?.Invoke(data);
    }

    [Message(MessageType.CmdOnClientConnected)]
    void ClientConnected(ClientData data)
    {
        OnOtherClientConnected?.Invoke(data);
    }

    [Message(MessageType.CmdOnClientPlayedCard)]
    void ClientPlayedCard(ClientIdentifier identifier)
    {
        OnClientPlayedCard?.Invoke(identifier.Guid);
    }

    [Message(MessageType.CmdSyncGameState)]
    void SyncGameState(GameState state)
    {
        OnGameStateChanged?.Invoke(State, state);
        State = state;
    }

    [Message(MessageType.CmdBeginVoting)]
    void BeginVoting()
    {
        OnVotingBegin?.Invoke();
    }

    [Message(MessageType.CmdBeginNewRound)]
    void BeginNewRound()
    {
        OnNewRoundBegin?.Invoke();
    }

    [Message(MessageType.CmdDrawCard)]
    void DrawCard(CardDefinition card)
    {
        OnDrawCard?.Invoke(card);
    }

    [Message(MessageType.CmdHeartbeat)]
    void Heartbeat()
    {
        CardsClient.Instance.Send(MessageType.RpcHeartbeatAck, new EmptyData(), false);
    }

    public ClientData ClientFromGuid(int cliGuid)
    {
        return State?.clients.Single(c => c.guid == cliGuid);
    }
}
