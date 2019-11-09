using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

[Serializable]
public struct CardDefinition : IMessageData
{
    [SerializeField] string m_cardText;
    public string CardText => m_cardText;
    [SerializeField] int m_pick;
    public int Pick => m_pick;

    public static implicit operator CardDefinition(string s) => new CardDefinition(s);

    [JsonConstructor]
    public CardDefinition(string text, int pick = 1)
    {
        m_cardText = Escape(text);
        m_pick = pick;
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
        m_pick = reader.ReadInt32();
        m_cardText = reader.ReadString();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(m_pick);
        writer.Write(m_cardText);
    }
}