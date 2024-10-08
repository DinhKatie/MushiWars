using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireHero : BaseHero
{
    protected override void Start()
    {
        base.Start();
        prefab = UnitPrefabs.fireHero;
        heroType = HeroTypes.Fire;
    }

    //Fire Boy can move the campfire on square in any cardinal direction without being next to it.
    public override void UseAbility()
    {
        return;
    }
}
