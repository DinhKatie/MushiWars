using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] public BaseUnit unitPrefab;
    [SerializeField] public BaseUnit swordUnitPrefab;
    [SerializeField] public BaseUnit gunUnitPrefab;

    private Dictionary<Vector3Int, BaseUnit> _unitsOnTiles = new Dictionary<Vector3Int, BaseUnit>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // Example: Spawning a unit on tile (0, 0) at the start
        Vector3Int startingTile = new Vector3Int(0, 0, 0);
        SpawnUnit(startingTile, unitPrefab);
        Vector3Int tile = new Vector3Int(1, 1, 0);
        SpawnUnit(tile, unitPrefab);
        tile = new Vector3Int(-1, -1, 0);
        SpawnUnit(tile, swordUnitPrefab);
        tile = new Vector3Int(-2, 0, 0);
        SpawnUnit(tile, gunUnitPrefab);
        TurnManager.Instance.StartTurn();
    }

    public void LogUnitsOnTiles()
    {
        foreach (var entry in _unitsOnTiles)
        {
            Debug.Log($"Tile: {entry.Key}, Unit: {entry.Value}");
        }
    }

    private void SpawnUnit(Vector3Int spawnTile, BaseUnit type)
    {
        // Check if the tile is valid and no unit is already there
        if (_tilemap.GetTile(spawnTile) != null && !_unitsOnTiles.ContainsKey(spawnTile))
        {
            // spawn Unit
            BaseUnit newUnit = Instantiate(type, _tilemap.GetCellCenterWorld(spawnTile), Quaternion.identity);

            newUnit.SetCurrentPosition(spawnTile);

            // Add the unit to the dictionary to track its position
            _unitsOnTiles[spawnTile] = newUnit;

            Debug.Log($"Unit spawned on tile {spawnTile}");
            newUnit.name = "Mushi " + _unitsOnTiles.Count;
            TurnManager.Instance.AddUnitToTurnSystem(newUnit);
        }
        else
        {
            Debug.Log($"Tile {spawnTile} is either invalid or already has a unit.");
        }
    }

    public void RemoveUnit(Vector3Int unitTile)
    {
        if (_unitsOnTiles.TryGetValue(unitTile, out BaseUnit unit))
        {
            _unitsOnTiles.Remove(unit.CurrentPosition);
            Destroy(unit.gameObject);
        }
    }

    // Method to get the unit at a specific tile
    public BaseUnit GetUnitAtTile(Vector3Int tilePosition)
    {
        if (_unitsOnTiles.TryGetValue(tilePosition, out BaseUnit unit))
        {
            return unit;
        } 
        return null;
    }

    public bool isTileValid(Vector3Int tile)
    {
        if (GetUnitAtTile(tile) != null)
            return false;
        return true;
    }

    public void MoveUnit(BaseUnit unit, Vector3Int newPosition)
    {
        if (!isTileValid(newPosition) || !unit.IsTurn || unit.MovementRange <= 0 || unit.CalculateMoveCost(newPosition) > unit.MovementRange) return;

        _unitsOnTiles.Remove(unit.CurrentPosition);
        _unitsOnTiles[newPosition] = unit;

        unit.Move(newPosition);
    }

    public void AttackUnit(BaseUnit attacker, Vector3Int hitUnit)
    {
        //One attack per turn
        if (attacker.HasAttacked || !attacker.IsTurn)
        {
            Debug.Log($"{attacker.name} has attacked already!");
            return;
        }

        //Ensure target is within the current unit's attack range
        List<Vector3Int> attackRanges = attacker.CalculateValidAttacks();
        if (attackRanges.Contains(hitUnit))
        {
            if (_unitsOnTiles.TryGetValue(hitUnit, out BaseUnit unit))
            {
                attacker.Attack(hitUnit);
                //Remove the killed unit from the turn system
                TurnManager.Instance.RemoveUnitFromTurnSystem(unit);
            }
            else
                Debug.Log($"No unit found at {hitUnit}");
        }
        else
            Debug.Log($"{hitUnit} is out of attack range");
    }

}
