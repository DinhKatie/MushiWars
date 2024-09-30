using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : BaseUnit
{
    public bool canRevive = true; //One unit revival per turn

    protected override void ResetStats()
    {
        movementRange = 0;
        attackRange = 0;
        hasAttacked = false;
    }

    public void ResetRevive()
    {
        canRevive = true;
    }

    public bool TryRevive(BaseUnit unit)
    {
        if (canRevive && unit.isDead)
        {
            //unit.Revive();
            canRevive = false; // Campfire has been used for this turn
            return true;
        }
        return false;
    }

    //Campfires cannot move or attack
    public override void Move(Vector3Int newPosition)
    {
        return;
    }

    public override void Attack(BaseUnit enemy)
    {
        return;
    }

    public bool IsUnitAdjacentToCampfire(BaseUnit unit)
    {
        return Vector3Int.Distance(unit.CurrentPosition, currPosition) == 1;
    }

    public List<Vector3Int> GetValidPushDirections()
    {
        List<Vector3Int> validDirections = new List<Vector3Int>();

        // Cardinal directions around the campfire
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1),  // Up
            new Vector3Int(0, -1), // Down
            new Vector3Int(-1, 0), // Left
            new Vector3Int(1, 0)   // Right
        };

        foreach (Vector3Int direction in directions)
        {
            Vector3Int newPosition = currPosition + direction;

            if (GridManager.Instance.GetTileAtPosition(newPosition)) // You need a function to check if the tile is free
            {
                validDirections.Add(direction);
            }
        }
        return validDirections;
    }
}

