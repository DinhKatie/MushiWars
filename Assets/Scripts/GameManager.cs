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
    private void SpawnSquad1()
    {
        Vector3Int heroSpawnTile = new Vector3Int(-5, -5, 0);
        BaseUnit heroUnit = UnitManager.Instance.SpawnUnit(heroSpawnTile, UnitPrefabs.fireHero);
        TurnManager.Instance.AddUnitToSquad(heroUnit, Squads.one);

        List<Vector3Int> swordUnits = new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(-4, -3, 0),
        };

        List<Vector3Int> gunUnits = new List<Vector3Int>
        {
            new Vector3Int(-3, -5, 0),
            new Vector3Int(-3, -4, 0),
        };

        foreach (var unit in swordUnits)
        {
            BaseUnit swordGuy = UnitManager.Instance.SpawnUnit(unit, UnitPrefabs.swordUnit);
            TurnManager.Instance.AddUnitToSquad(swordGuy, Squads.one);
        }

        foreach (var unit in gunUnits)
        {
            BaseUnit gunGuy = UnitManager.Instance.SpawnUnit(unit, UnitPrefabs.gunUnit);
            TurnManager.Instance.AddUnitToSquad(gunGuy, Squads.one);
        }
    }

    private void SpawnSquad2()
    {
        Vector3Int heroSpawnTile = new Vector3Int(5, 5, 0);
        BaseUnit heroUnit = UnitManager.Instance.SpawnUnit(heroSpawnTile, UnitPrefabs.fireHero);
        TurnManager.Instance.AddUnitToSquad(heroUnit, Squads.two);

        List<Vector3Int> swordUnits = new List<Vector3Int>
        {
            new Vector3Int(0, 1, 0),
            new Vector3Int(4, 3, 0),
        };

        List<Vector3Int> gunUnits = new List<Vector3Int>
        {
            new Vector3Int(3, 5, 0),
            new Vector3Int(3, 4, 0),
        };

        foreach(var unit in swordUnits)
        {
            BaseUnit swordGuy = UnitManager.Instance.SpawnUnit(unit, UnitPrefabs.swordUnit);
            TurnManager.Instance.AddUnitToSquad(swordGuy, Squads.two);
        }

        foreach (var unit in gunUnits)
        {
            BaseUnit gunGuy = UnitManager.Instance.SpawnUnit(unit, UnitPrefabs.gunUnit);
            TurnManager.Instance.AddUnitToSquad(gunGuy, Squads.two);
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
