using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[Serializable]
public class GameState : IMessageData
{
    public GamePhase phase;
    public CardDefinition currentBlackCard = new CardDefinition();
    public int currentCzar;

    public List<ClientData> clients = new List<ClientData>();
    public GameSettings currentSettings;
    public CardPackList cardPacks;

    public int CurrentCzarGuid => clients[currentCzar].guid;
    public int CurrentServerOwner => clients.First().guid;

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

        currentSettings.FromBytes(reader);

        cardPacks = new CardPackList();
        var packCount = reader.ReadInt32();
        for (var i = 0; i < packCount; i++)
        {
            var id = reader.ReadString();
            var name = reader.ReadString();
            cardPacks.packs.Add(new CardPack {id = id, name = name});
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((int)phase);
        writer.Write(currentCzar);
        currentBlackCard.Write(writer);

        writer.Write(clients.Count);
        foreach (var cli in clients) cli.Write(writer);

        currentSettings.Write(writer);

        writer.Write(cardPacks.packs.Count);
        foreach (var pack in cardPacks.packs)
        {
            writer.Write(pack.id);
            writer.Write(pack.name);
        }
    }
}

public enum GamePhase
{
    Playing, Voting, Waiting
}