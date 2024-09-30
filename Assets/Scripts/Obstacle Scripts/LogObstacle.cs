using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static LogObstacle;

public class LogObstacle : BaseObstacle
{
    private RotationState rotationState;
    public RotationState GetRotation => rotationState;

    protected override void UpdateOccupiedTiles()
    {
        occupiedTiles = new List<Vector3Int>();

        // Update the occupied tiles based on the rotation state
        if (rotationState == RotationState.Horizontal)
        {
            // Log spans 2 tiles horizontally
            occupiedTiles.Add(currPosition);
            occupiedTiles.Add(currPosition + new Vector3Int(1, 0, 0));
        }
        else if (rotationState == RotationState.Vertical)
        {
            // Log spans 2 tiles vertically
            occupiedTiles.Add(currPosition);
            occupiedTiles.Add(currPosition + new Vector3Int(0, -1, 0));
        }

    }
    public override void SetRotation(RotationState rotation)
    {
        rotationState = rotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
        }
    }

    private void Rotate()
    {
        if (rotationState == RotationState.Horizontal)
        {
            rotationState = RotationState.Vertical;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            Debug.Log("Rotating to Vertical: " + transform.rotation.eulerAngles);
        }
            
        else
        {
            rotationState = RotationState.Horizontal;
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        List<Vector3Int> oldTiles = occupiedTiles;
        UpdateOccupiedTiles();
        List<Vector3Int> newTiles = occupiedTiles;
        GridManager.Instance.UpdateObstacleList(oldTiles, newTiles);

    }

    public enum RotationState
    { 
        Horizontal,
        Vertical,
    }

}
