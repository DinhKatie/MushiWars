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
        SpawnUnit(startingTile);
        Vector3Int tile = new Vector3Int(1, 1, 0);
        SpawnUnit(tile);
        TurnManager.Instance.StartTurn();
    }

    public void LogUnitsOnTiles()
    {
        foreach (var entry in _unitsOnTiles)
        {
            Debug.Log($"Tile: {entry.Key}, Unit: {entry.Value}");
        }
    }

    private void SpawnUnit(Vector3Int spawnTile)
    {
        // Check if the tile is valid and no unit is already there
        if (_tilemap.GetTile(spawnTile) != null && !_unitsOnTiles.ContainsKey(spawnTile))
        {
            // spawn Unit
            BaseUnit newUnit = Instantiate(unitPrefab);
            newUnit.transform.position = _tilemap.GetCellCenterWorld(spawnTile);
            
            newUnit.currPosition = spawnTile;

            // Add the unit to the dictionary to track its position
            _unitsOnTiles[spawnTile] = newUnit;

            Debug.Log($"Unit spawned on tile {spawnTile}");
            TurnManager.Instance.AddUnitToTurnSystem(newUnit);
        }
        else
        {
            Debug.LogWarning($"Tile {spawnTile} is either invalid or already has a unit.");
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
        if (!isTileValid(newPosition) || unit.IsTurn() == false || unit.GetMoveRange() <= 0 || unit.CalculateMoveCost(newPosition) > unit.GetMoveRange()) return;

        _unitsOnTiles.Remove(unit.currPosition);
        _unitsOnTiles[newPosition] = unit;

        unit.Move(newPosition);
    }



}
