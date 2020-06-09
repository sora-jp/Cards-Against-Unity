using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusUIManager : MonoBehaviour
{
    public PlayerStatusUI prefab;

    List<PlayerStatusUI> m_players = new List<PlayerStatusUI>();

    void Awake()
    {
        ClientImplementation.OnGameStateChanged += GameStateChanged;
    }

    void OnDestroy()
    {
        ClientImplementation.OnGameStateChanged -= GameStateChanged;
    }

    void GameStateChanged(GameState last, GameState cur)
    {
        Debug.Log("Syncing player ui");
        for (var i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        foreach (var client in cur.clients)
        {
            Debug.Log($"Creating player ui for {client.guid}");
            Instantiate(prefab, transform).linkedClient = client;
        }
    }
}
