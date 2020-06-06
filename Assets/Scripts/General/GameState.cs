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
    public int currentCzar;

    public int CurrentCzarGuid => m_clients?[currentCzar].guid ?? clients[currentCzar].guid;
    public List<ClientData> clients = new List<ClientData>();
    List<Client> m_clients;

    public void SetClientSource(List<Client> c)
    {
        m_clients = c;
    }

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
        writer.Write(m_clients.Count);
        foreach (var cli in m_clients) new ClientData(cli).Write(writer);
    }
}

public enum GamePhase
{
    Playing, Voting, Waiting
}