using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState GameState;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ChangeState(GameState state)
    {
        GameState = state;
        switch (state)
        {
            case GameState.SpawnSquad1:
                break;
            case GameState.SpawnSquad2:
                break;
            case GameState.Player1Turn:
                break;
            case GameState.Player2Turn:
                break;
        }

    }
}

public enum GameState
{
    GenerateGrid = 0,
    SpawnSquad1 = 1,
    SpawnSquad2 = 2,
    Player1Turn = 3,
    Player2Turn = 4
}
