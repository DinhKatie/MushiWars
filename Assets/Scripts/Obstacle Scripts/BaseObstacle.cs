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

    public virtual void SetRotation(RotationState rotation)
    {
        return;
    }

    protected virtual void UpdateOccupiedTiles()
    {
        occupiedTiles.Add(currPosition);
    }
}
