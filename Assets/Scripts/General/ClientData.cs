using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientData : IMessageData
{
    public int Guid { get; set; }

    public ClientData() { }

    public ClientData(int idx)
    {
        Guid = idx;
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