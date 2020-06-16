using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour
{
    public ClientData linkedClient;
    public TextMeshProUGUI playerName, playerScore;
    public Image playerCrownImage;

    void Update()
    {
        playerCrownImage.enabled = ClientImplementation.Instance.State.CurrentCzarGuid == linkedClient.guid;
        playerName.text = !string.IsNullOrEmpty(linkedClient.name) ? linkedClient.name : linkedClient.guid.ToString();
        playerScore.text = linkedClient.score.ToString();
    }
}
