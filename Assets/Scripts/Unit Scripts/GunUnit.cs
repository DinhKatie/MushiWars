using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunUnit : BaseUnit
{
    protected override void Start()
    {
        base.Start();
        ResetStats();
        prefab = UnitPrefabs.gunUnit;
    }

    protected override void ResetStats()
    {
        movementRange = 1;
        attackRange = 2; //Except not point blank
        hasAttacked = false;
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
}
