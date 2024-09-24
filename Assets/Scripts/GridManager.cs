using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private TileBase _highlightTile;
    [SerializeField] private Tilemap _highlightTilemap;
    private Vector3Int _previousHoverTilePosition; // Store the previously hovered tile position

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
                HighlightTile(tilePosition);
            }
        }
        else
        {
            // If no tile is hovered, deselect the previous one
            _highlightTilemap.SetTile(_previousHoverTilePosition, null);
            _previousHoverTilePosition = tilePosition; // Reset previous hover position
        }
    }

    private void HighlightTile(Vector3Int tilePosition)
    {
        // Set the highlight tile on the Tilemap
        _highlightTilemap.SetTile(tilePosition, _highlightTile);
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
            {
                // Select the tile
                SelectTile(tilePosition);
            }
        }
    }

    private void SelectTile(Vector3Int tilePosition)
    {
        // Deselect previous tile
        if (_selectedTilePosition != tilePosition)
        {
            DeselectTile(_selectedTilePosition);
        }

        _selectedTilePosition = tilePosition;

        // Highlight the selected tile
        _tilemap.SetTile(_selectedTilePosition, _highlightTile);
    }

    private void DeselectTile(Vector3Int tilePosition)
    {
        // Restore the original tile if it was highlighted
        if (_highlightTile != null && tilePosition != null)
        {
            TileBase originalTile = GetTileAtPosition(tilePosition); // Get the original tile
            if (originalTile != null)
            {
                _tilemap.SetTile(tilePosition, originalTile); // Restore the original tile
            }
        }
    }


    // Function to get a tile at a specific position
    public TileBase GetTileAtPosition(Vector3Int position)
    {
        return _tilemap.GetTile(position); // Get the tile at the specified grid position
    }

    // Function to set a tile at a specific position
    public void SetTileAtPosition(Vector3Int position, TileBase tile)
    {
        _tilemap.SetTile(position, tile); // Set a tile at the specified position
    }
}
