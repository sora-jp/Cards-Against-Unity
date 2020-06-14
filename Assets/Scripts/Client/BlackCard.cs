using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlackCard : MonoBehaviour
{
    public TextMeshProUGUI pickText;
    public GameObject pickContainer;

    Card m_card;

    void Awake()
    {
        m_card = GetComponent<Card>();
    }

    void Update()
    {
        pickText.text = m_card.CardDef.Pick.ToString();
        pickContainer.SetActive(m_card.CardDef.Pick > 1);
    }
}
