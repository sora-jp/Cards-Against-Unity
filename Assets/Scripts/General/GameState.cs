using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameState : IMessageData
{
    public CardDefinition currentBlackCard = new CardDefinition();
    public int currentCardCzar;
    public List<int> playerScores = new List<int>();
    public List<string> playerNames = new List<string>();


    public void FromBytes(BinaryReader reader)
    {
        currentBlackCard.FromBytes(reader);
        var len = reader.ReadInt32();
        for (var i = 0; i < len; i++)
        {
            playerScores.Add(reader.ReadInt32());
        }
        len = reader.ReadInt32();
        for (var i = 0; i < len; i++)
        {
            playerNames.Add(reader.ReadString());
        }
    }

    public void Write(BinaryWriter writer)
    {
        currentBlackCard.Write(writer);
        writer.Write(playerScores.Count);
        foreach (var sc in playerScores) writer.Write(sc);
        writer.Write(playerNames.Count);
        foreach (var nm in playerNames) writer.Write(nm);
    }
}