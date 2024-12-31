using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SwordUnit : BaseUnit
{
    protected override void Start()
    {
        base.Start();
        prefab = UnitPrefabs.swordUnit;
    }
    protected override List<Vector3Int> GetAttackRange()
    {
        return Utilities.GetValidTiles(this, "square", attackRange);
    }
}
