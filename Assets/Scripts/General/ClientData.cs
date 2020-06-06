using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class ClientData : IMessageData
{
    public int guid, score, currentCardAmt;
    public string name;

    public ClientData() { }

    public ClientData(Client client)
    {
        guid = client.guid;
        score = client.score;
        name = client.name;
        currentCardAmt = client.currentCards.Count;
    }

    public void FromBytes(BinaryReader reader)
    {
        guid = reader.ReadInt32();
        name = reader.ReadString();
        score = reader.ReadInt32();
        currentCardAmt = reader.ReadInt32();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(guid);
        writer.Write(name);
        writer.Write(score);
        writer.Write(currentCardAmt);
    }
}