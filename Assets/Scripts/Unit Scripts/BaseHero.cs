using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHero : BaseUnit
{
    protected override void Start()
    {
        base.Start();
        health = 3;
    }
}
