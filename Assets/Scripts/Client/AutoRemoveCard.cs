using UnityEngine;

public class AutoRemoveCard : MonoBehaviour
{
    Card m_card;

    void Awake()
    {
        m_card = GetComponent<Card>();
        ClientImplementation.OnShouldRemoveCard += RemoveIfCardEqual;
    }

    void OnDestroy()
    {
        ClientImplementation.OnShouldRemoveCard -= RemoveIfCardEqual;
    }

    void RemoveIfCardEqual(CardDefinition obj)
    {
        if (m_card.CardDef == obj) Destroy(gameObject);
    }
}
