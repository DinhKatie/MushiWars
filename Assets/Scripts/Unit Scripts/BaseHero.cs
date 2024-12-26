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

    public void IncrementHealth(int amount = 1)
    {
        health += amount;
    }

    public abstract void UseAbility();

    protected override void OnDeath()
    {
        GameManager.Instance.GameEnd(squad);
    }

}
