using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : BaseUnit
{
    public bool canRevive = true; //One unit revival per turn

    public void ResetRevive()
    {
        canRevive = true;
    }

    public bool TryRevive(BaseUnit unit)
    {
        if (canRevive && unit.isDead)
        {
            //unit.Revive();
            canRevive = false; // Campfire has been used for this turn
            return true;
        }
        return false;
    }

    //Campfires cannot move or attack
    public override void Move(Vector3Int newPosition)
    {
        return;
    }

    public override void Attack(BaseUnit enemy)
    {
        return;
    }

    

}

