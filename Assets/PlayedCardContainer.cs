using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayedCardContainer : MonoBehaviour
{
    public GameObject subHolderPrefab;
    public GameObject cardPrefab;

    Dictionary<int, Transform> clientToContainer;

    void Awake()
    {
        clientToContainer = new Dictionary<int, Transform>();
        ClientImplementation.OnClientPlayedCard += OnRemoteCardPlayed;
        CardDrag.OnCardPlayed += OnCardPlayed;
    }

    void OnCardPlayed(Card c, Transform cTransform)
    {
        var container = GetContainer(ClientImplementation.Instance.MyGuid);
        cTransform.SetParent(container);
        cTransform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
    }

    void OnRemoteCardPlayed(int cliGuid)
    {
        var container = GetContainer(cliGuid);
        var card = Instantiate(cardPrefab, container);
        card.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);
    }

    Transform GetContainer(int playerGuid)
    {
        Transform obj;
        if (clientToContainer.ContainsKey(playerGuid)) obj = clientToContainer[playerGuid];
        else
        {
            obj = Instantiate(subHolderPrefab, transform).transform;
            clientToContainer.Add(playerGuid, obj);
        }

        return obj;
    }
}
