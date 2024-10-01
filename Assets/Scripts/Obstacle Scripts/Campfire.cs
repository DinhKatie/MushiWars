using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : BaseUnit
{
    public bool canRevive = true; //One unit revival per turn
    public List<BaseUnit> graveyard = new List<BaseUnit>(); //Keep track of dead units

    protected override void ResetStats()
    {
        movementRange = 0;
        attackRange = 0;
        hasAttacked = false;
    }

    public void RegisterDeadUnit(BaseUnit unit) { graveyard.Add(unit); }

    public void UnregisterDeadUnit(BaseUnit unit) {  graveyard.Remove(unit); }

    public void ListGraveyard()
    {
        for (int i = 0; i < graveyard.Count; i++)
        {
            string className = graveyard[i].GetType().Name;
            Debug.Log($"{i}: {className}");
        }
    }

    public void ResetRevive()
    {
        canRevive = true;
    }

    public bool TryRevive(BaseUnit unit)
    {
        if (canRevive && graveyard.Contains(unit))
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

