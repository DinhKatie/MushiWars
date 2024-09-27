using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public List<BaseUnit> units; 
    private int currentUnitIndex = 0; // Track whose turn it is


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            EndTurn();
    }

    // Start the turn of the current unit
    public void StartTurn()
    {
        BaseUnit currentUnit = units[currentUnitIndex];
        currentUnit.StartTurn();
    }

    // End the current unit's turn and move to the next
    public void EndTurn()
    {
        units[currentUnitIndex].EndTurn();
        currentUnitIndex = (currentUnitIndex + 1) % units.Count; // Loop through the units
        StartTurn();
    }

    public void AddUnitToTurnSystem(BaseUnit unit)
    {
        units.Add(unit);
    }

    public void RemoveUnitFromTurnSystem(BaseUnit unit)
    {
        units.Remove(unit);
        //Adjust unit index to match with new list size
        if (units.Count > 0)
            currentUnitIndex = currentUnitIndex % units.Count;
        else
            currentUnitIndex = 0;
    }
}
