using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class ShrinkBoard : MonoBehaviour
{
    [Header("Island")]
    public Tilemap tilemap; //The island itself
    public Tile leftEdgeTile;
    public Tile rightEdgeTile;
    public Tile bottomEdgeTile;
    public Tile topEdgeTile;
    public Tile topRightCorner;
    public Tile topLeftCorner;

    [Header("Ridges")]
    public Tilemap unselectableIsland; //The ridges at the bottom of the island
    public Tile leftRidge;
    public Tile middleRidge;
    public Tile rightRidge;
    public Tile secondLayerLeftRidge;
    public Tile secondLayerRightRidge;
    public Tile secondLayerMiddleRidge;
    public Tile thirdLayerLeftRidge;
    public Tile thirdLayerRightRidge;
    public Tile thirdLayerMiddleRidge;

    private int boardSize = 10;
    private int minX;
    private int maxX;
    private int minY;
    private int maxY;

    private void Awake()
    {
        minX = 0;
        maxX = boardSize - 1;
        minY = 0;
        maxY = boardSize - 1;
    }

    public void BoardShrink()
    {
        if (boardSize == 2) return;
        Debug.Log($"MinX: {minX}, MaxX: {maxX}, MinY: {minY}, MaxY: {maxY}");

        //Remove the outer layer of tiles
        for (int x = minX; x <= maxX; x++)
        {
            tilemap.SetTile(new Vector3Int(x, minY, 0), null);
            tilemap.SetTile(new Vector3Int(x, maxY, 0), null);
        }
        for (int y = minY; y <= maxY; y++)
        {
            tilemap.SetTile(new Vector3Int(minX, y, 0), null);
            tilemap.SetTile(new Vector3Int(maxX, y, 0), null);
        }

        RemoveLayerOfRidge();

        //Adjust the bounds for the new size
        minX++;
        maxX--;
        minY++;
        maxY--;

        Debug.Log($"MinX: {minX}, MaxX: {maxX}, MinY: {minY}, MaxY: {maxY}");

        //new corner tiles
        tilemap.SetTile(new Vector3Int(minX, maxY, 0), topLeftCorner);
        tilemap.SetTile(new Vector3Int(maxX, maxY, 0), topRightCorner);
        tilemap.SetTile(new Vector3Int(minX, minY, 0), leftEdgeTile);
        tilemap.SetTile(new Vector3Int(maxX, minY, 0), rightEdgeTile);

        //new edge tiles
        for (int x = minX + 1; x < maxX; x++)
        {
            tilemap.SetTile(new Vector3Int(x, minY, 0), bottomEdgeTile);
            tilemap.SetTile(new Vector3Int(x, maxY, 0), topEdgeTile);
        }
        for (int y = minY + 1; y < maxY; y++)
        {
            tilemap.SetTile(new Vector3Int(minX, y, 0), leftEdgeTile);
            tilemap.SetTile(new Vector3Int(maxX, y, 0), rightEdgeTile);
        }

        AddLayerOfRidge(leftRidge, rightRidge, middleRidge, 1);
        AddLayerOfRidge(secondLayerLeftRidge, secondLayerRightRidge, secondLayerMiddleRidge, 2);
        AddLayerOfRidge(thirdLayerLeftRidge, thirdLayerRightRidge, thirdLayerMiddleRidge, 3);

        boardSize -= 2;
        Debug.Log($"The board size after the shrink is {boardSize}x{boardSize}");
    }

    private void AddLayerOfRidge(Tile leftTile, Tile rightTile, Tile middleTile, int depth)
    {
        //Add new ridges on bottom of the island
        unselectableIsland.SetTile(new Vector3Int(minX, minY - depth, 0), leftTile);
        unselectableIsland.SetTile(new Vector3Int(maxX, minY - depth, 0), rightTile);

        for (int x = minX + 1; x < maxX; x++)
        {
            unselectableIsland.SetTile(new Vector3Int(x, minY - depth, 0), middleTile);
        }
    }

    private void RemoveLayerOfRidge()
    {
        for (int i = 1; i <= 3; i++)
        {
            //Remove the ridges at the bottom of the island
            unselectableIsland.SetTile(new Vector3Int(minX, minY - i, 0), null);
            unselectableIsland.SetTile(new Vector3Int(maxX, minY - i, 0), null);

            for (int x = minX + 1; x < maxX; x++)
            {
                unselectableIsland.SetTile(new Vector3Int(x, minY - i, 0), null);
            }
        }
        
    }
}
