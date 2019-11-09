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
    public GameObject frontFace;
    public GameObject backFace;

    void UpdateUi()
    {
        text.SetText(CardDef.CardText);
    }

    void Update()
    {
        var backFacing = transform.localRotation.eulerAngles.y > 90 && transform.localRotation.eulerAngles.y < 270;
        frontFace.SetActive(!backFacing);
        backFace.SetActive(backFacing);
    }
}