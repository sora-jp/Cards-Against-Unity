using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;

[Serializable]
public class GameState : IMessageData
{
    public GamePhase phase;
    public CardDefinition currentBlackCard = new CardDefinition();
    public int currentCardCzar;
    public List<ClientData> clients = new List<ClientData>();
    List<Client> m_clients;

    public void SetClientSource(List<Client> clients)
    {
        m_clients = clients;
    }

    public void FromBytes(BinaryReader reader)
    {
        if (clients == null) clients = new List<ClientData>();

        currentCardCzar = reader.ReadInt32();
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
        writer.Write(currentCardCzar);
        currentBlackCard.Write(writer);
        writer.Write(clients.Count);
        foreach (var cli in m_clients) new ClientData(cli).Write(writer);
    }
}

public enum GamePhase
{
    Playing, Voting, Waiting
}