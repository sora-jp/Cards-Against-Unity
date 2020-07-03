using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public struct CardPack
{
    public string name;
    [JsonProperty(PropertyName = "__unused_1")] [JsonIgnore] public string id;
    [JsonProperty(PropertyName = "__unused_2")] [JsonIgnore] public List<CardDefinition> white;
    [JsonProperty(PropertyName = "__unused_3")] [JsonIgnore] public List<CardDefinition> black;

    [JsonProperty(PropertyName = "white")] List<int> m_whiteIdx;
    [JsonProperty(PropertyName = "black")] List<int> m_blackIdx;

    internal void SetCards(List<CardDefinition> whiteCards, List<CardDefinition> blackCards)
    {
        white = m_whiteIdx.Select(i => whiteCards[i]).ToList();
        black = m_blackIdx.Select(i => blackCards[i]).ToList();
    }
}