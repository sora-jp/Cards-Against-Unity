using System.Collections.Generic;
using Newtonsoft.Json;

public class CardCollection
{
    /*[JsonProperty(PropertyName = "whiteCards")] */public List<CardDefinition> whiteCards;
    /*[JsonProperty(PropertyName = "blackCards")] */public List<CardDefinition> blackCards;
}