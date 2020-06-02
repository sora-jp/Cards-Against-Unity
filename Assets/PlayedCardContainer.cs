using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayedCardContainer : MonoBehaviour
{
    public GameObject subHolderPrefab;
    public Card cardPrefab;

    public Dictionary<int, List<Card>> clientToCardInstance = new Dictionary<int, List<Card>>();

    Dictionary<int, Transform> m_clientToContainer;

    void Awake()
    {
        m_clientToContainer = new Dictionary<int, Transform>();
        ClientImplementation.OnClientPlayedCard += OnCardPlayed;
    }

    void OnCardPlayed(int cliGuid)
    {
        var container = GetContainer(cliGuid);
        var card = Instantiate(cardPrefab, container);
        card.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
        if (!clientToCardInstance.ContainsKey(cliGuid)) clientToCardInstance.Add(cliGuid, new List<Card> {card});
        else clientToCardInstance[cliGuid].Add(card);
    }

    Transform GetContainer(int playerGuid)
    {
        Transform obj;
        if (m_clientToContainer.ContainsKey(playerGuid)) obj = m_clientToContainer[playerGuid];
        else
        {
            obj = Instantiate(subHolderPrefab, transform).transform;
            m_clientToContainer.Add(playerGuid, obj);
        }

        return obj;
    }
}
