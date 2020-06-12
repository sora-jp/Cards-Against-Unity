// If this code works, it was written by Sora
// Otherwise, I don't know who wrote it

using System.IO;

public class ClientName : IMessageData
{
    public string Name { get; set; }

    public void FromBytes(BinaryReader reader)
    {
        Name = reader.ReadString();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Name);
    }
}