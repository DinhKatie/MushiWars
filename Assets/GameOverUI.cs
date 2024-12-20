using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public TMP_Text winningTeamText;

    void Start()
    {
        // Display the winning team
        if (GameManager.Instance != null)
        {
            winningTeamText.text = "Player " + GameManager.Instance.winningTeam + " Wins!";
        }
    }
}
