using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private TileBase _highlightTile;
    [SerializeField] private TileBase _outlineTile;
    [SerializeField] private Tilemap _highlightTilemap;
    [SerializeField] private Tilemap _outlineTilemap;
    

    
    private Vector3Int _previousHoverTilePosition;

    private Vector3Int _selectedTilePosition;

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
        // Perform a raycast from the mouse position to detect tiles
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePosition = _tilemap.WorldToCell(mousePosition);

            // Check if the tile is valid
            TileBase tile = GetTileAtPosition(tilePosition);
            if (tile != null)
                SelectTile(tilePosition);
        }
        else if (Input.GetMouseButtonDown(1)) //Cancel selection on right mouse click
        {
            _outlineTilemap.SetTile(_selectedTilePosition, null);
            _selectedTilePosition = new Vector3Int(-1, -1, -1);
        }
    }

    private void SelectTile(Vector3Int tilePosition)
    {
        // Deselect previous tile
        if (_selectedTilePosition != tilePosition)
            _outlineTilemap.SetTile(_selectedTilePosition, null);

        BaseUnit unit = UnitManager.Instance.GetUnitAtTile(_selectedTilePosition);
        //If a unit is selected, move the unit to the new position
        if (unit != null)
        {
            UnitManager.Instance.MoveUnit(unit, tilePosition);
        }

        _selectedTilePosition = tilePosition;

        // Highlight the selected tile
        _outlineTilemap.SetTile(_selectedTilePosition, _outlineTile);
        _highlightTilemap.SetTile(_selectedTilePosition, null);
        //Debug.Log("Selected tile: " +  _selectedTilePosition);
        UnitManager.Instance.GetUnitAtTile(_selectedTilePosition);
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
