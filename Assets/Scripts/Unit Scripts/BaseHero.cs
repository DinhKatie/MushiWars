using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseHero : BaseUnit
{
    public HeroTypes heroType;

    public bool CanUseHexAndSupports = true;
    public bool Cursed = false;

    public bool UsesHexAndSupports => CanUseHexAndSupports;

    public bool IsCursed => Cursed;
    public void SetCursed(bool curse) {  Cursed = curse; }

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

    protected override void ResetStats()
    {
        base.ResetStats();
        if (Cursed)
        {
            CanUseHexAndSupports = false;
            Cursed = false;
        }
        else
        {
            CanUseHexAndSupports = true;
        }
            
    }

}
