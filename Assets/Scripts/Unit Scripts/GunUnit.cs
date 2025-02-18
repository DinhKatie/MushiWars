using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunUnit : BaseUnit
{
    protected override void Start()
    {
        base.Start();
        maxMoveRange = 1;
        maxAttackRange = 2;
        ResetStats();
        prefab = UnitPrefabs.gunUnit;
    }

    protected override List<Vector3Int> GetAttackRange()
    {
        List<Vector3Int> attackRanges = new List<Vector3Int>
        {
            currPosition + new Vector3Int(0, attackRange, 0),
            currPosition + new Vector3Int(0, -attackRange, 0),
            currPosition + new Vector3Int(-attackRange, 0, 0),
            currPosition + new Vector3Int(attackRange, 0, 0),
            currPosition + new Vector3Int(-attackRange, attackRange, 0), //Diagonals
            currPosition + new Vector3Int(attackRange, attackRange, 0),   
            currPosition + new Vector3Int(-attackRange, -attackRange, 0), 
            currPosition + new Vector3Int(attackRange, -attackRange, 0)   
        };
        return attackRanges;
    }

    public override bool HasNotActed()
    {
        return (movementRange == 1 && attackRange == 2 && hasAttacked == false);
    }
}
