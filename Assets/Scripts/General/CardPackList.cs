using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public struct CardPackList
{
    public List<CardPack> packs;

    [JsonProperty(PropertyName = "whiteCards")] List<CardDefinition> m_whiteCards;
    [JsonProperty(PropertyName = "blackCards")] List<CardDefinition> m_blackCards;
    [JsonProperty(PropertyName = "order")] List<string> m_packs;

    [JsonExtensionData]
    IDictionary<string, JToken> m_additionalData;

    [OnDeserialized]
    void FillSets(StreamingContext ctx)
    {
        packs = new List<CardPack>();

        foreach (var packId in m_packs)
        {
            var pack = m_additionalData[packId].ToObject<CardPack>();
            pack.id = packId;
            pack.SetCards(m_whiteCards, m_blackCards);

            packs.Add(pack);
        }
    }

    public CardCollection GetCollectionForPacks(params string[] packs)
    {
        return new CardCollection
        {
            whiteCards = this.packs.Where(p => packs.Contains(p.id)).SelectMany(p => p.white).ToList(),
            blackCards = this.packs.Where(p => packs.Contains(p.id)).SelectMany(p => p.black).ToList()
        };
    }
}