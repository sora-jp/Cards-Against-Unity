using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
internal class CardCollection
{
    [JsonProperty(PropertyName = "whiteCards")] public List<CardDefinition> WhiteCards;
    [JsonProperty(PropertyName = "blackCards")] public List<CardDefinition> BlackCards;
}