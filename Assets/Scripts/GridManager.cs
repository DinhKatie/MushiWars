using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    private Vector3Int _previousHoverTilePosition;

    private Vector3Int _previousTileSelection = new Vector3Int(-1, -1, -1); //Default if no tile is selected

    public static GridManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _highlightTilemap.ClearAllTiles();
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
        _previousTileSelection = new Vector3Int(-1, -1, -1);
    }

    private void SelectTile(Vector3Int tilePosition)
    {
        // Deselect previous tile
        if (_previousTileSelection != tilePosition)
            _outlineTilemap.SetTile(_previousTileSelection, null);

        BaseUnit unit = UnitManager.Instance.GetUnitAtTile(_previousTileSelection);
        BaseUnit newUnit = UnitManager.Instance.GetUnitAtTile(tilePosition);
        
        //If the new position has a unit on it, try to attack it.
        if (newUnit != null && unit != null)
        {
            UnitManager.Instance.AttackUnit(unit, tilePosition);
            Debug.Log($"{unit.name} attacked {newUnit.name}!");
        }
        //If a unit is selected, move the unit to the new position
        else if (unit != null)
        {
            UnitManager.Instance.MoveUnit(unit, tilePosition);
        }

        _previousTileSelection = tilePosition;

        // Highlight the selected tile
        _outlineTilemap.SetTile(tilePosition, _outlineTile);
        _highlightTilemap.SetTile(tilePosition, null);
        //Debug.Log("Selected tile: " +  _selectedTilePosition);
        UnitManager.Instance.GetUnitAtTile(tilePosition);
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
