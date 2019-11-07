using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public struct CardDefinition : IMessageData
{
    [JsonProperty(PropertyName = "text")] public string CardText { get; set; }
    [DefaultValue(1), JsonProperty(PropertyName = "pick", DefaultValueHandling = DefaultValueHandling.Populate)] public int Pick { get; set; }

    public static implicit operator CardDefinition(string s) => new CardDefinition {CardText = s, Pick = 1};

    public override string ToString()
    {
        return $"{CardText} (pick {Pick})";
    }

    public void FromBytes(BinaryReader reader)
    {
        Pick = reader.ReadInt32();
        CardText = reader.ReadString();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Pick);
        writer.Write(CardText);
    }
}