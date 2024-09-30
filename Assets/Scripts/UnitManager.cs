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
    [SerializeField] public BaseUnit fireHeroPrefab;

    private Dictionary<Vector3Int, BaseUnit> _unitsOnTiles = new Dictionary<Vector3Int, BaseUnit>();
    private Dictionary<UnitPrefabs, BaseUnit> unitPrefabsDict;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        unitPrefabsDict = new Dictionary<UnitPrefabs, BaseUnit>
        {
            { UnitPrefabs.unit, unitPrefab },
            { UnitPrefabs.swordUnit, swordUnitPrefab },
            { UnitPrefabs.gunUnit, gunUnitPrefab },
            { UnitPrefabs.fireHero, fireHeroPrefab }
        };
    }

    public void LogUnitsOnTiles()
    {
        foreach (var entry in _unitsOnTiles)
        {
            Debug.Log($"Tile: {entry.Key}, Unit: {entry.Value}");
        }
    }

    public BaseUnit SpawnUnit(Vector3Int spawnTile, UnitPrefabs unitType)
    {
        // Check if the tile is valid and no unit is already there
        if (_tilemap.GetTile(spawnTile) != null && !_unitsOnTiles.ContainsKey(spawnTile))
        {
            // Retrieve the prefab based on the enum type
            BaseUnit prefabToSpawn = unitPrefabsDict[unitType];

            // Spawn Unit
            BaseUnit newUnit = Instantiate(prefabToSpawn, _tilemap.GetCellCenterWorld(spawnTile), Quaternion.identity);

            newUnit.SetCurrentPosition(spawnTile);

            // Add the unit to the dictionary to track its position
            _unitsOnTiles[spawnTile] = newUnit;

            Debug.Log($"Unit spawned on tile {spawnTile}");
            newUnit.name = "Mushi " + _unitsOnTiles.Count;

            return newUnit;
        }
        Debug.Log($"Tile {spawnTile} is either invalid or already has a unit.");
        return null;
    }

    public void RemoveUnit(Vector3Int unitTile)
    {
        if (_unitsOnTiles.TryGetValue(unitTile, out BaseUnit unit))
        {
            _unitsOnTiles.Remove(unit.CurrentPosition);
            //Remove the killed unit from the turn system
            TurnManager.Instance.RemoveUnitFromTurnSystem(unit);
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
        if (!isTileValid(newPosition) || unit.MovementRange <= 0 || unit.CalculateMoveCost(newPosition) > unit.MovementRange) return;
        if (GridManager.Instance.IsObstacleTile(newPosition))
        {
            Debug.Log("That is an obstacle.");
            return;
        }

        _unitsOnTiles.Remove(unit.CurrentPosition);
        _unitsOnTiles[newPosition] = unit;

        unit.Move(newPosition);
    }

    public void AttackUnit(BaseUnit attacker, BaseUnit hitUnit)
    {
        //One attack per turn
        if (attacker.HasAttacked)
        {
            Debug.Log($"{attacker.name} has attacked already!");
            return;
        }

        //Ensure target is within the current unit's attack range
        List<Vector3Int> attackRanges = attacker.CalculateValidAttacks();
        if (attackRanges.Contains(hitUnit.CurrentPosition))
        {
            if (_unitsOnTiles.TryGetValue(hitUnit.CurrentPosition, out BaseUnit unit))
            {
                attacker.Attack(hitUnit);
            }
            else
                Debug.Log($"No unit found at {hitUnit}");
        }
        else
            Debug.Log($"{hitUnit} is out of attack range");
    }

    // Update highlights when grid changes
    public void UpdateUnitHighlights()
    {
        foreach (var unit in _unitsOnTiles.Values)
        {
            if (unit != null)
                unit.HighlightValidMoves();
        }
    }

    public void GetUnitHighlights(BaseUnit unit)
    {
        unit.HighlightValidMoves();
    }

    public void ResetTeam(List<BaseUnit> squad)
    {
        foreach (var unit in squad)
        {
            unit.Reset();
        }
    }

}

public enum UnitPrefabs
{
    unit = 0,
    swordUnit = 1,
    gunUnit = 2,
    fireHero = 3,
}

