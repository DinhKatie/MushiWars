using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState GameState;
    public Squads winningTeam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
            
        else
            Destroy(gameObject);

        
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ChangeState(GameState.SpawnSquad1);
            ChangeState(GameState.SpawnSquad2);
        }
    }

    public void ChangeState(GameState state)
    {
        GameState = state;
        switch (state)
        {
            case GameState.SpawnSquad1:
                SpawnSquad1();
                break;
            case GameState.SpawnSquad2:
                SpawnSquad2();
                break;
            case GameState.Player1Turn:
                break;
            case GameState.Player2Turn:
                break;
        }

    }
    private void SpawnSquad(Vector3Int campfireTile, Vector3Int heroTile, Vector3Int swordTile, Vector3Int gunTile, List<Vector3Int> normalUnits, Squads team)
    {
        UnitManager.Instance.SpawnUnit(campfireTile, UnitPrefabs.campfire, team);

        UnitManager.Instance.SpawnUnit(heroTile, UnitPrefabs.fireHero, team);

        UnitManager.Instance.SpawnUnit(swordTile, UnitPrefabs.swordUnit, team);

        UnitManager.Instance.SpawnUnit(gunTile, UnitPrefabs.gunUnit, team);

        foreach (var unit in normalUnits)
            UnitManager.Instance.SpawnUnit(unit, UnitPrefabs.unit, team);
        
    }

    private void SpawnSquad1()
    {
        Vector3Int campfireTile = new Vector3Int(1, 1, 0);
        Vector3Int heroSpawnTile = new Vector3Int(0, 0, 0);
        Vector3Int swordTile = new Vector3Int(4, 5, 0);
        Vector3Int gunTile = new Vector3Int(1, 2, 0);
        List<Vector3Int> normalUnits = new List<Vector3Int>
        {
            new Vector3Int(2, 0, 0),
            new Vector3Int(0, 2, 0),
        };

        SpawnSquad(campfireTile, heroSpawnTile, swordTile, gunTile, normalUnits, Squads.one);
    }

    private void SpawnSquad2()
    {
        Vector3Int campfireTile = new Vector3Int(8, 8, 0);
        Vector3Int heroSpawnTile = new Vector3Int(9, 9, 0);
        Vector3Int swordTile = new Vector3Int(4, 6, 0);
        Vector3Int gunTile = new Vector3Int(8, 7, 0);
        List<Vector3Int> normalUnits = new List<Vector3Int>
        {
            new Vector3Int(7, 9, 0),
            new Vector3Int(9, 7, 0),
        };

        SpawnSquad(campfireTile, heroSpawnTile, swordTile, gunTile, normalUnits, Squads.two);


    }

    public void GameEnd(Squads winningSquad)
    {
        winningTeam = winningSquad;
        SceneManager.LoadScene("GameOver");
    }
}

