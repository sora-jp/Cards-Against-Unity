using System;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class GameState : IMessageData
{
    public GamePhase phase;
    public CardDefinition currentBlackCard = new CardDefinition();
    public int currentCzar;

    public int CurrentCzarGuid => clients[currentCzar].guid;
    public List<ClientData> clients = new List<ClientData>();

    public void FromBytes(BinaryReader reader)
    {
        if (clients == null) clients = new List<ClientData>();

        phase = (GamePhase)reader.ReadInt32();
        currentCzar = reader.ReadInt32();
        currentBlackCard.FromBytes(reader);
        var len = reader.ReadInt32();
        for (var i = 0; i < len; i++)
        {
            var cli = new ClientData();
            cli.FromBytes(reader);
            clients.Add(cli);
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((int)phase);
        writer.Write(currentCzar);
        currentBlackCard.Write(writer);
        writer.Write(clients.Count);
        foreach (var cli in clients) cli.Write(writer);
    }
}

public enum GamePhase
{
    Playing, Voting, Waiting
}