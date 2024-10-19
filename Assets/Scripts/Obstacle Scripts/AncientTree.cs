using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AncientTree : BaseObstacle
{
    protected override void UpdateOccupiedTiles()
    {
        //Where parent is the top left tile
        occupiedTiles = new List<Vector3Int>
        {
            currPosition,
            currPosition + new Vector3Int(1, 0, 0),
            currPosition + new Vector3Int(0, -1, 0),
            currPosition + new Vector3Int(1, -1, 0)
        };
    }
}
