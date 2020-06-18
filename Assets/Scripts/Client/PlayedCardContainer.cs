using System.Collections.Generic;
using UnityEngine;

public class PlayedCardContainer : MonoBehaviour
{
    public CardContainerForPlayer subHolderPrefab;
    public Card cardPrefab;

    Dictionary<int, Transform> m_clientToContainer;

    void Awake()
    {
        m_clientToContainer = new Dictionary<int, Transform>();
        //ClientImplementation.OnGameStateChanged += SyncCardsWithGameState;
        ClientImplementation.OnClientPlayedCard += OnCardPlayed;
        ClientImplementation.OnNewRoundBegin += ClearContainers;
    }


    void OnDestroy()
    {
        //ClientImplementation.OnGameStateChanged -= SyncCardsWithGameState;
        ClientImplementation.OnClientPlayedCard -= OnCardPlayed;
        ClientImplementation.OnNewRoundBegin -= ClearContainers;
    }

    //void SyncCardsWithGameState(GameState prev, GameState cur)
    //{
    //    foreach (var client in cur.clients)
    //    {
    //        if (client.currentCardAmt > 0)
    //        {
    //            var container = GetContainer(client);
    //            for (var i = 0; i < client.currentCardAmt - (container.childCount - 1); i++)
    //            {
    //                OnCardPlayed(client.guid);
    //            }

    //            for (var i = 0; i < (container.childCount - 1) - client.currentCardAmt; i++)
    //            {
    //                Destroy(container.GetChild(i+1).gameObject);
    //            }
    //        }
    //        else
    //        {
    //            Destroy(GetContainer(client).gameObject);
    //            m_clientToContainer.Remove(client.guid);
    //        }
    //    }
    //}

    void ClearContainers()
    {
        foreach (var pair in m_clientToContainer)
        {
            Destroy(pair.Value.gameObject);
        }

        m_clientToContainer.Clear();
    }

    void OnCardPlayed(int cliGuid)
    {
        var container = GetContainer(ClientImplementation.Instance.ClientFromGuid(cliGuid));
        var card = Instantiate(cardPrefab, container);
        card.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
    }

    Transform GetContainer(ClientData client)
    {
        Transform obj;
        if (m_clientToContainer.ContainsKey(client.guid)) obj = m_clientToContainer[client.guid];
        else
        {
            var container = Instantiate(subHolderPrefab, transform);
            container.linkedClient = client;
            container.GetComponentInChildren<ClientVoteButton>().linkedClient = container.linkedClient;
            obj = container.transform;
            m_clientToContainer.Add(client.guid, obj);
        }

        return obj;
    }
}
