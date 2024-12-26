using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static List<Vector3Int> GetValidTiles(BaseUnit unit, bool includesDiagonals, int range = 1)
    {
        Vector3Int currPosition = unit.CurrentPosition;
        List<Vector3Int> tiles = new List<Vector3Int>();

        //Orthogonals
        for (int i = 1; i <= range; i++)
        {
            tiles.Add(currPosition + Vector3Int.up * i);
            tiles.Add(currPosition + Vector3Int.down * i);
            tiles.Add(currPosition + Vector3Int.left * i);
            tiles.Add(currPosition + Vector3Int.right * i);
        }

        if (includesDiagonals)
        {
            for (int i = 1; i <= range; i++)
            {
                tiles.Add(currPosition + (Vector3Int.up + Vector3Int.left) * i);
                tiles.Add(currPosition + (Vector3Int.up + Vector3Int.right) * i);
                tiles.Add(currPosition + (Vector3Int.down + Vector3Int.left) * i);
                tiles.Add(currPosition + (Vector3Int.down + Vector3Int.right) * i);
            }
        }
        return tiles;
    }
}
