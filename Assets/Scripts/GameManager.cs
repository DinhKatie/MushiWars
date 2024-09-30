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
        ChangeState(GameState.SpawnSquad1);
        ChangeState(GameState.SpawnSquad2);
        TurnManager.Instance.StartTurn();
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
    private void SpawnSquad(Vector3Int heroSpawnTile, Vector3Int swordTile, Vector3Int gunTile, List<Vector3Int> normalUnits, Squads team)
    {
        BaseUnit heroUnit = UnitManager.Instance.SpawnUnit(heroSpawnTile, UnitPrefabs.fireHero);
        TurnManager.Instance.AddUnitToSquad(heroUnit, team);

        BaseUnit swordGuy = UnitManager.Instance.SpawnUnit(swordTile, UnitPrefabs.swordUnit);
        TurnManager.Instance.AddUnitToSquad(swordGuy, team);

        BaseUnit gunGuy = UnitManager.Instance.SpawnUnit(gunTile, UnitPrefabs.gunUnit);
        TurnManager.Instance.AddUnitToSquad(gunGuy, team);

        foreach (var unit in normalUnits)
        {
            BaseUnit basicGuy = UnitManager.Instance.SpawnUnit(unit, UnitPrefabs.unit);
            TurnManager.Instance.AddUnitToSquad(basicGuy, team);
        }
    }

    private void SpawnSquad1()
    {
        Vector3Int heroSpawnTile = new Vector3Int(-5, -5, 0);
        Vector3Int swordTile = new Vector3Int(-3, -4, 0);
        Vector3Int gunTile = new Vector3Int(-4, -3, 0);
        List<Vector3Int> normalUnits = new List<Vector3Int>
        {
            new Vector3Int(-3, -5, 0),
            new Vector3Int(-5, -3, 0),
        };

        SpawnSquad(heroSpawnTile, swordTile, gunTile, normalUnits, Squads.one);
    }

    private void SpawnSquad2()
    {
        Vector3Int heroSpawnTile = new Vector3Int(5, 5, 0);
        Vector3Int swordTile = new Vector3Int(3, 4, 0);
        Vector3Int gunTile = new Vector3Int(4, 3, 0);
        List<Vector3Int> normalUnits = new List<Vector3Int>
        {
            new Vector3Int(3, 5, 0),
            new Vector3Int(5, 3, 0),
        };

        SpawnSquad(heroSpawnTile, swordTile, gunTile, normalUnits, Squads.two);
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
