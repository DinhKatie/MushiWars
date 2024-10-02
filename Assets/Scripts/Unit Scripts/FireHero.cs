using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireHero : BaseHero
{
    protected override void Start()
    {
        base.Start();
        prefab = UnitPrefabs.fireHero;
    }
}
