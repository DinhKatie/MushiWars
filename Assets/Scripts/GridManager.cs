using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.UI.CanvasScaler;

public class GridManager : MonoBehaviour
{
    [SerializeField] public Tilemap _tilemap;

    [SerializeField] private Tilemap _highlightTilemap;
    [SerializeField] private Tilemap _outlineTilemap;
    [SerializeField] private Tilemap _validMovesMap;

    [SerializeField] private TileBase _highlightTile;
    [SerializeField] private TileBase _outlineTile;
    [SerializeField] private TileBase _validMoveTile;
    [SerializeField] private TileBase _validAttackTile;
    [SerializeField] private TileBase _campfirePushTile;

    [SerializeField] private BaseObstacle _obstaclePrefab;
    [SerializeField] private LogObstacle _logObstaclePrefab;


    public List<Vector3Int> _obstacles;
    private Dictionary<Obstacle, BaseObstacle> obstaclesPrefabsDict;

    private Vector3Int _previousHoverTilePosition;

    private Vector3Int _previousTileSelection = new Vector3Int(-1, -1, -1); //Default if no tile is selected

    public static GridManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        obstaclesPrefabsDict = new Dictionary<Obstacle, BaseObstacle>
        {
            { Obstacle.log, _logObstaclePrefab },
            { Obstacle.tree, _obstaclePrefab },

        };
    }

    private void Start()
    {
        _highlightTilemap.ClearAllTiles();
        _obstacles = new List<Vector3Int>();
    }

    private void Update()
    {
        HandleTileHover();
        HandleTileSelection();
    }

    private void HandleTileHover()
    {
        // Perform a raycast from the mouse position to detect tiles
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int tilePosition = _tilemap.WorldToCell(mousePosition);

        // Check if the hovered tile is valid
        TileBase hoveredTile = GetTileAtPosition(tilePosition);
        if (hoveredTile != null)
        {
            // Highlight the tile if it's not already highlighted
            if (tilePosition != _previousHoverTilePosition)
            {
                // Deselect the previously hovered tile
                _highlightTilemap.SetTile(_previousHoverTilePosition, null);
                _previousHoverTilePosition = tilePosition;

                // Highlight the current tile
                _highlightTilemap.SetTile(tilePosition, _highlightTile);
            }
        }
        else
        {
            // If no tile is hovered, deselect the previous one
            _highlightTilemap.SetTile(_previousHoverTilePosition, null);
            _previousHoverTilePosition = tilePosition; // Reset previous hover position
        }
    }

    private void HandleTileSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePosition = _tilemap.WorldToCell(mousePosition);

            // Check if the tile is valid
            if (GetTileAtPosition(tilePosition) != null)
                SelectTile(tilePosition);
        }
        else if (Input.GetMouseButtonDown(1)) //Cancel selection on right mouse click
            Deselect();
    }

    public void Deselect()
    {
        _outlineTilemap.ClearAllTiles();
        ClearValidMoves();
        _previousTileSelection = new Vector3Int(-1, -1, -1);
    }

    private void SelectTile(Vector3Int tilePosition)
    {
        // Deselect previous tile
        if (_previousTileSelection != tilePosition)
            _outlineTilemap.SetTile(_previousTileSelection, null);

        // Highlight the selected tile
        _outlineTilemap.SetTile(tilePosition, _outlineTile);
        _highlightTilemap.SetTile(tilePosition, null);

        BaseUnit previousUnit = UnitManager.Instance.GetUnitAtTile(_previousTileSelection);
        BaseUnit newUnit = UnitManager.Instance.GetUnitAtTile(tilePosition);

        // If a unit is clicked and it's the current squad's turn
        if (newUnit != null)
        {
            // If no unit was previously selected, highlight the clicked unit's movement options
            if (previousUnit == null && TurnManager.Instance.isUnitInCurrentSquad(newUnit))
            {
                UnitManager.Instance.GetUnitHighlights(newUnit);
                Debug.Log($"{newUnit.name} selected. Highlighting move options.");
            }
            else if (previousUnit != null && newUnit != previousUnit) //previously selected a unit, and now clicked another unit. Check for attack or switching selection.
            {
                if (!TurnManager.Instance.isUnitInCurrentSquad(newUnit)) //Unit clicked is not in the squad. Attack them.
                {
                    UnitManager.Instance.AttackUnit(previousUnit, newUnit);
                    Debug.Log($"{previousUnit.name} attacked {newUnit.name}!");
                }
                else //Unit Clicked is in the squad. Switch selection.
                {
                    UnitManager.Instance.GetUnitHighlights(newUnit);
                    Debug.Log($"{newUnit.name} selected. Switching selection and highlights.");
                }
            }
        }
        //Otherwise, if clicked a unit then clicked an empty tile, move the unit.
        else if (previousUnit != null && newUnit == null && TurnManager.Instance.isUnitInCurrentSquad(previousUnit))
        {
            UnitManager.Instance.MoveUnit(previousUnit, tilePosition);
        }

        _previousTileSelection = tilePosition;

    }

    public bool isCampfirePushable(BaseUnit unit)
    {
        Campfire fire = GetCampfireNearby(unit);
        if (fire == null) return false;

        Vector3Int pushDirection = -(unit.CurrentPosition - fire.CurrentPosition);
        Vector3Int targetTile = fire.CurrentPosition + pushDirection;
        if (UnitManager.Instance.GetUnitAtTile(targetTile) == null)
        {
            _validMovesMap.SetTile(fire.CurrentPosition, _campfirePushTile);
            return true;
        }
        return false;
    }

    public Campfire GetCampfireNearby(BaseUnit unit)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(0, 1),  // Up
        new Vector3Int(0, -1), // Down
        new Vector3Int(-1, 0), // Left
        new Vector3Int(1, 0)   // Right
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int adjacentPos = unit.CurrentPosition + dir;
            Campfire campfire = GetCampfireAtPosition(adjacentPos);

            if (campfire != null)
            {
                Debug.Log($"Returning {campfire.name}");
                return campfire;
            }
        }
        return null; // No campfire nearby
    }

    public Campfire GetCampfireAtPosition(Vector3Int position)
    {
        BaseUnit unit = UnitManager.Instance.GetUnitAtTile(position);
        if (unit is Campfire campfire)
            return campfire;
        else 
            return null;

    }

    public void HighlightValidMoves(List<Vector3Int> moves)
    {
        foreach (var move in moves)
        {
            _validMovesMap.SetTile(move, _validMoveTile);
        }
    }

    public void HighlightValidAttacks(List<Vector3Int> attacks)
    {
        foreach (var att in attacks)
        {
            _validMovesMap.SetTile(att, _validAttackTile);
        }
    }

    public void ClearValidMoves()
    {
        _validMovesMap.ClearAllTiles();
    }

    // Instantiate obstacle and add to obstacles list
    public void SpawnObstacle(Vector3Int spawnTile, Obstacle prefabToSpawn, RotationState? rotationState = null)
    {
        Vector3 worldPosition = _tilemap.GetCellCenterWorld(spawnTile);
        BaseObstacle prefab = obstaclesPrefabsDict[prefabToSpawn];

        // Instantiate the obstacle
        BaseObstacle obstacle;

        if (prefab is LogObstacle)
        {
            if (rotationState == RotationState.Horizontal)
                obstacle = Instantiate(_logObstaclePrefab, worldPosition, Quaternion.Euler(0, 0, 90));
            else
                obstacle = Instantiate(_logObstaclePrefab, worldPosition, Quaternion.Euler(0, 0, 0));
            obstacle.SetRotation(rotationState.Value);

        }
        else
        {
            obstacle = Instantiate(prefab, worldPosition, Quaternion.identity);
        }
        obstacle.SetPosition(spawnTile);

        // Label the tiles it takes up as obstacles
        List<Vector3Int> occupiedTiles = obstacle.GetOccupiedTiles;
        foreach (var tile in occupiedTiles)
        {
            _obstacles.Add(tile);
        }
    }

    public void UpdateObstacleList(List<Vector3Int> oldTiles, List<Vector3Int> newTiles)
    {
        foreach (var tile in oldTiles)
        {
            if (_obstacles.Contains(tile))
                _obstacles.Remove(tile);
        }

        foreach (var tile in newTiles)
        {
            if (!_obstacles.Contains(tile))
                _obstacles.Add(tile);
        }
    }

    public bool IsObstacleTile(Vector3Int tile)
    {
        return _obstacles.Contains(tile);
    }

    // Get the tile at the specified grid position
    public TileBase GetTileAtPosition(Vector3Int position)
    {
        return _tilemap.GetTile(position); 
    }

    // Set a tile at a specific position
    public void SetTileAtPosition(Vector3Int position, TileBase tile)
    {
        _tilemap.SetTile(position, tile); // Set a tile at the specified position
    }
}



