using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SwordUnit : BaseUnit
{
    protected override List<Vector3Int> GetAttackRange()
    {
        List<Vector3Int> attackRanges = base.GetAttackRange();
        List<Vector3Int> diagonals = new List<Vector3Int>
        {
            // Diagonal directions
            currPosition + new Vector3Int(-1, 1, 0),  // Northwest
            currPosition + new Vector3Int(1, 1, 0),   // Northeast
            currPosition + new Vector3Int(-1, -1, 0), // Southwest
            currPosition + new Vector3Int(1, -1, 0)   // Southeast
        };
        attackRanges.AddRange(diagonals);

        return attackRanges;
    }
}
