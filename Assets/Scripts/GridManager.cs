using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] public Tilemap _tilemap;
    [SerializeField] private Tilemap _highlightTilemap;
    [SerializeField] private Tilemap _outlineTilemap;
    [SerializeField] private Tilemap _validMovesMap;

    [Header("Tiles")]
    [SerializeField] private TileBase _highlightTile;
    [SerializeField] private TileBase _outlineTile;
    [SerializeField] private TileBase _validMoveTile;
    [SerializeField] private TileBase _validAttackTile;
    [SerializeField] private TileBase _campfirePushTile;

    [Header("Obstacles")]
    [SerializeField] private BaseObstacle _treeObstaclePrefab;
    [SerializeField] private BaseObstacle _rockObstaclePrefab;
    [SerializeField] private LogObstacle _logObstaclePrefab;
    [SerializeField] private BaseObstacle _bambooObstaclePrefab;


    public List<Vector3Int> _obstacles = new List<Vector3Int>();
    private Dictionary<Obstacle, BaseObstacle> obstaclesPrefabsDict;

    private Vector3Int _previousHoverTilePosition;

    private Vector3Int _previousTileSelection = new Vector3Int(-1, -1, -1); //Default if no tile is selected
    public bool avoidSelect = false; // For cases where a selection coincides with another selection (e.g. cards like Blast Stomp)

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
            { Obstacle.tree, _treeObstaclePrefab },
            { Obstacle.rock, _rockObstaclePrefab },
            { Obstacle.bamboo, _bambooObstaclePrefab },

        };
    }

    private void Start()
    {
        _highlightTilemap.ClearAllTiles();
        InitializeDefaultObstacles();
    }

    private void Update()
    {
        //if (!EventSystem.current.IsPointerOverGameObject())
        //{
         HandleTileHover();
         HandleTileSelection();
        //}
    }

    private void InitializeDefaultObstacles()
    {
        SpawnObstacle(new Vector3Int(2, 4, 0), Obstacle.rock);
        SpawnObstacle(new Vector3Int(6, 7, 0), Obstacle.log, RotationState.Horizontal);
        SpawnObstacle(new Vector3Int(0, 8, 0), Obstacle.log, RotationState.Vertical);
        SpawnObstacle(new Vector3Int(4, 1, 0), Obstacle.tree);
        SpawnObstacle(new Vector3Int(6, 5, 0), Obstacle.bamboo);
    }

    private void HandleTileHover()
    {
        // Detect tiles the mouse is over
        Vector3Int tilePosition = GetMouseTilePosition();

        // Deselect the previously hovered tile
        _highlightTilemap.SetTile(_previousHoverTilePosition, null);
        _previousHoverTilePosition = tilePosition;

        // Check if the hovered tile is valid
        if (GetTileAtPosition(tilePosition) != null)
        {
            _highlightTilemap.SetTile(tilePosition, _highlightTile);
        }

    }

    private void HandleTileSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Vector3Int tilePosition = GetMouseTilePosition();

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
        StopAllCoroutines();
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
        Debug.Log($"{tilePosition} selected.");
        _previousTileSelection = tilePosition;

        if (avoidSelect) { return; }

        //if (!TurnManager.Instance._playerControlsOn) return;

        // If a unit is clicked and it's the current squad's turn
        if (newUnit != null)
        {
            HandleUnitSelection(previousUnit, newUnit);
        }
        //Otherwise, if clicked a unit then clicked an empty tile, move the unit.
        else if (IsValidMove(previousUnit, newUnit))
        {
            UnitManager.Instance.MoveUnit(previousUnit, tilePosition);
        }
        

    }

    private void HandleUnitSelection(BaseUnit previousUnit, BaseUnit newUnit)
    {
        if (IsInitialUnitSelection(previousUnit, newUnit))
        {
            HighlightUnitOptions(newUnit);
        }
        else if (IsCampfirePush(previousUnit, newUnit))
        {
            AttemptCampfirePush(previousUnit, (Campfire)newUnit);
        }
        else if (IsAttackScenario(previousUnit, newUnit))
        {
            UnitManager.Instance.AttackUnit(previousUnit, newUnit);
            Debug.Log($"{previousUnit.name} attacked {newUnit.name}!");
            ClearValidMoves();
        }
        else if (TurnManager.Instance.isUnitInCurrentSquad(newUnit)) //New Unit clicked is in the current squad. Switch selection.
        {
            HighlightUnitOptions(newUnit);
        }
    }

    private bool IsInitialUnitSelection(BaseUnit previousUnit, BaseUnit newUnit)
    {
        return previousUnit == null && TurnManager.Instance.isUnitInCurrentSquad(newUnit);
    }

    private bool IsCampfirePush(BaseUnit previousUnit, BaseUnit newUnit)
    {
        return previousUnit != null && newUnit is Campfire campfire &&
               TurnManager.Instance.isUnitInCurrentSquad(campfire);
    }

    private bool IsAttackScenario(BaseUnit previousUnit, BaseUnit newUnit)
    {
        return previousUnit != null && !TurnManager.Instance.isUnitInCurrentSquad(newUnit) &&
               TurnManager.Instance.isUnitInCurrentSquad(previousUnit);
    }

    private void HighlightUnitOptions(BaseUnit unit)
    {
        UnitManager.Instance.GetUnitHighlights(unit);
        Debug.Log($"{unit.name} selected. Highlighting move options.");
    }


    //----- CAMPFIRE LOGIC ------

    private void AttemptCampfirePush(BaseUnit previousUnit, Campfire campfire)
    {
        Vector3Int pushDirection = isCampfirePushable(previousUnit);

        // Check if campfire can be pushed and if it's a valid tile
        if (pushDirection != new Vector3Int(-1, -1, -1)) // nonValidTile check
        {
            UnitManager.Instance.PushCampfire(previousUnit, campfire, pushDirection);
            Debug.Log($"{previousUnit.name} is pushing the campfire.");
        }
        else
            HighlightUnitOptions(campfire);
    }

    public Vector3Int isCampfirePushable(BaseUnit unit)
    {
        Campfire fire = GetCampfireNearby(unit);
        if (fire == null) return new Vector3Int(-1,-1,-1);

        Vector3Int pushDirection = -(unit.CurrentPosition - fire.CurrentPosition);
        Vector3Int targetTile = fire.CurrentPosition + pushDirection;

        if (!IsOccupied(targetTile) && unit.MovementRange > 0)
        {
            _validMovesMap.SetTile(fire.CurrentPosition, _campfirePushTile);
            return targetTile;
        }
        return new Vector3Int(-1, -1, -1);
    }

    public Campfire GetCampfireNearby(BaseUnit unit)
    {
        List<Vector3Int> directions = Utilities.GetValidTiles(unit, "orthogonal", 1);

        foreach (Vector3Int dir in directions)
        {
            Campfire campfire = GetCampfireAtPosition(dir);

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
        return UnitManager.Instance.GetUnitAtTile(position) as Campfire;
    }

    // ----------------------------------


    private bool IsValidMove(BaseUnit previousUnit, BaseUnit newUnit)
    {
        return previousUnit != null && newUnit == null && TurnManager.Instance.isUnitInCurrentSquad(previousUnit);
    }


    public Vector3Int GetMouseTilePosition()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return _tilemap.WorldToCell(mouseWorldPosition);
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

    public void HighlightRevivalTiles(List<Vector3Int> revivals)
    {
        foreach (var tile in revivals)
            _validMovesMap.SetTile(tile, _outlineTile);
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


    public bool IsOccupied(Vector3Int tile)
    {
        return (IsObstacleTile(tile) || UnitManager.Instance.GetUnitAtTile(tile) != null || GetTileAtPosition(tile) == null);
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

    public void HighlightOutlineTiles(List<Vector3Int> outlines)
    {
        foreach (var tile in outlines)
            _validMovesMap.SetTile(tile, _outlineTile);
    }
}



