using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class CardPack
{
    public string name;
    [JsonIgnore] public string id;
    [JsonIgnore] public List<CardDefinition> white;
    [JsonIgnore] public List<CardDefinition> black;

    [JsonProperty(PropertyName = "white")] List<int> m_whiteIdx;
    [JsonProperty(PropertyName = "black")] List<int> m_blackIdx;

    internal void SetCards(List<CardDefinition> whiteCards, List<CardDefinition> blackCards)
    {
        white = m_whiteIdx.Select(i => whiteCards[i]).ToList();
        black = m_blackIdx.Select(i => blackCards[i]).ToList();
    }
}