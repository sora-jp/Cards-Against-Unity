using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour
{
    CardDefinition _cardDef;

    public CardDefinition CardDef
    {
        get => _cardDef;
        set
        {
            _cardDef = value;
            UpdateUi();
        }
    }

    public TextMeshProUGUI text;

    void UpdateUi()
    {
        text.SetText(CardDef.CardText);
    }
}
