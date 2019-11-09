using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackCardUiManager : MonoBehaviour
{
    public Card blackCard;

    void Awake()
    {
        ClientImplementation.OnGameStateChanged += OnGameStateChange;
    }

    private void OnGameStateChange(GameState oldState, GameState newState)
    {
        blackCard.CardDef = newState.currentBlackCard;
    }
}
