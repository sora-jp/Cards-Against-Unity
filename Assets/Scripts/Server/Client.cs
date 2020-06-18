using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class Client
{
    public int id;
    public int guid;
    public int score;
    public string name = "";
    public List<CardDefinition> hand = new List<CardDefinition>();
    public List<CardDefinition> currentCards = new List<CardDefinition>();

    public Client(int id)
    {
        this.id = id;
        guid = ServerImplementation.Instance.GenerateGuid();
    }

    public void FillHand(int targetAmt)
    {
        while (hand.Count < targetAmt) DrawCard();
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
        CardsServer.Instance.Send(id, MessageType.Heartbeat, new EmptyData(), false);
    }

    public void SendId()
    {
        CardsServer.Instance.Send(id, MessageType.CmdSetGuid, new ClientIdentifier(guid));
    }

    public ClientData ToClientData()
    {
        return new ClientData
        {
            guid = guid,
            score = score,
            name = name,
            currentCardAmt = currentCards.Count
        };
    }
}