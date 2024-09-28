using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunUnit : BaseUnit
{
    protected override void Start()
    {
        movementRange = 1;
        attackRange = 2; //Except not point blank
        hasAttacked = false;
    }

    public override void StartTurn()
    {
        Debug.Log($"It's Mushi on {currPosition}'s turn");
        movementRange = 1;
        attackRange = 2; 
        hasAttacked = false;
        turn = true;
        HighlightValidMoves();
    }
    protected override List<Vector3Int> GetAttackRange()
    {
        List<Vector3Int> attackRanges = new List<Vector3Int>
        {
            currPosition + new Vector3Int(0, 2, 0),
            currPosition + new Vector3Int(0, -2, 0),
            currPosition + new Vector3Int(-2, 0, 0),
            currPosition + new Vector3Int(2, 0, 0),
            currPosition + new Vector3Int(-2, 2, 0), //Ask Kevin if gun guys attack diagonals
            currPosition + new Vector3Int(2, 2, 0),   
            currPosition + new Vector3Int(-2, -2, 0), 
            currPosition + new Vector3Int(2, -2, 0)   
        };
        return attackRanges;
    }
}
