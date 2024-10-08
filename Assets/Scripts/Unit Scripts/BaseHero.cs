using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseHero : BaseUnit
{
    public HeroTypes heroType;

    protected override void Start()
    {
        base.Start();
        health = 3;
    }

    public abstract void UseAbility();
}
