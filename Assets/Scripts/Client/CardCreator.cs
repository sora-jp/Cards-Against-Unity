﻿using UnityEngine;

public class CardCreator : MonoBehaviour
{
    public Card prefab;
    public Transform overrideParent;

    void Awake()
    {
        ClientImplementation.OnDrawCard += CreateCard;
    }

    void OnDestroy()
    {
        ClientImplementation.OnDrawCard -= CreateCard;
    }

    void CreateCard(CardDefinition card)
    {
        var obj = Instantiate(prefab, overrideParent ?? transform);
        obj.CardDef = card;
    }
}
