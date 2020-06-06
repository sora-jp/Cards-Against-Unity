using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RevealCardData : IMessageData
{
    public int clientGuid, cardIndex;
    public bool hasCardDefinition;
    public CardDefinition cardData;
    
    public void FromBytes(BinaryReader reader)
    {
        clientGuid = reader.ReadInt32();
        cardIndex = reader.ReadInt32();
        hasCardDefinition = reader.ReadBoolean();
        if (hasCardDefinition) cardData.FromBytes(reader);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(clientGuid);
        writer.Write(cardIndex);
        writer.Write(hasCardDefinition);
        if (hasCardDefinition) cardData.Write(writer);
    }
}