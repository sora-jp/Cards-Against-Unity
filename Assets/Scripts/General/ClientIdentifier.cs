using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientIdentifier : IMessageData
{
    public int Guid { get; set; }

    public ClientIdentifier() { }

    public ClientIdentifier(int guid)
    {
        Guid = guid;
    }

    public void FromBytes(BinaryReader reader)
    {
        Guid = reader.ReadInt32();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Guid);
    }
}