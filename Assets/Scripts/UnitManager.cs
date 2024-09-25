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
            Debug.Log($"Unit at tile {tilePosition} retrieved");
            return unit;
        }
            
        return null;
    }
}
