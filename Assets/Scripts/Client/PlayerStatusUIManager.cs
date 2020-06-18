using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStatusUIManager : MonoBehaviour
{
    public PlayerStatusUI prefab;

    readonly Dictionary<int, PlayerStatusUI> m_players = new Dictionary<int, PlayerStatusUI>();

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
        var toCreate = last != null ? cur.clients.Except(last.clients) : cur.clients;
        var toDelete = last != null ? last.clients.Except(cur.clients) : Enumerable.Empty<ClientData>();

        foreach (var cli in toCreate)
        {
            m_players[cli.guid] = CreatePlayerUi(cli);
        }

        foreach (var cli in toDelete)
        {
            Destroy(m_players[cli.guid].gameObject);
            m_players.Remove(cli.guid);
        }

        var clientsByScore = cur.clients.OrderByDescending(c => c.score).ThenBy(c => c.name).ToArray();

        for (var i = 0; i < clientsByScore.Length; i++)
        {
            m_players[clientsByScore[i].guid].transform.SetSiblingIndex(i);
            m_players[clientsByScore[i].guid].linkedClient = clientsByScore[i];
        }
    }

    PlayerStatusUI CreatePlayerUi(ClientData cli)
    {
        Debug.Log($"Creating player ui for {cli.guid}");
        var statusUi = Instantiate(prefab, transform);
        statusUi.linkedClient = cli;
        return statusUi;
    }
}
