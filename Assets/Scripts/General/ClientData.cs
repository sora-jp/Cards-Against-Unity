using System;
using System.IO;
using System.Linq;

[Serializable]
public partial class ClientData : IMessageData, IEquatable<ClientData>
{
    public int guid, score, currentCardAmt;
    public string name;

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