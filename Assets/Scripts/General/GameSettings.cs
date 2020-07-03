using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public struct GameSettings : IMessageData
{
    public string[] enabledPacks;
    public int winningScore;
    public bool endlessMode;
    public int maxPlayerAmt;

    public void FromBytes(BinaryReader reader)
    {
        enabledPacks = Enumerable.Range(0, reader.ReadInt32()).Select(_ => reader.ReadString()).ToArray();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(enabledPacks.Length);
        foreach (var pack in enabledPacks)
        {
            writer.Write(pack);
        }
    }
}
