using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

internal class CardCollection
{
    [Preserve]
    public CardCollection() { }

    [JsonProperty(PropertyName = "whiteCards")] public List<CardDefinition> WhiteCards;
    [JsonProperty(PropertyName = "blackCards")] public List<CardDefinition> BlackCards;
}