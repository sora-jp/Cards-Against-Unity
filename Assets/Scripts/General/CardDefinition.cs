using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public struct CardDefinition : IMessageData
{
    [SerializeField] string _cardText;
    public string CardText => _cardText;
    [SerializeField] int _pick;
    public int Pick => _pick;

    public static implicit operator CardDefinition(string s) => new CardDefinition(s);

    [JsonConstructor]
    public CardDefinition(string text, int pick = 1)
    {
        _cardText = Escape(text);
        _pick = pick;
    }

    static string Escape(string text)
    {
        text = text.Replace("<small>", "<size=16>");
        text = text.Replace("</small>", "</size>");
        text = text.Replace("<br/>", "\n");
        text = text.Replace("_", "_____");
        text = WebUtility.HtmlDecode(text);
        return text;
    }

    public override string ToString()
    {
        return $"{CardText} (pick {Pick})";
    }

    public void FromBytes(BinaryReader reader)
    {
        _pick = reader.ReadInt32();
        _cardText = reader.ReadString();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(_pick);
        writer.Write(_cardText);
    }
}