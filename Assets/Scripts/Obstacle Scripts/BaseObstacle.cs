using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObstacle : MonoBehaviour
{
    public List<Vector3Int> occupiedTiles = new List<Vector3Int>();
    protected Vector3Int currPosition;

    public Vector3Int CurrPosition => currPosition;
    public List<Vector3Int> GetOccupiedTiles => occupiedTiles;

    public void SetPosition(Vector3Int newPosition)
    {
        currPosition = newPosition;
        UpdateOccupiedTiles();
    }

    protected virtual void UpdateOccupiedTiles()
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
