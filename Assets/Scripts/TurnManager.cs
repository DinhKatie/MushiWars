using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public List<BaseUnit> units;

    public List<BaseUnit> player1Squad;
    public List<BaseUnit> player2Squad;

    private Dictionary<Squads, List<BaseUnit>> squadsDict;
    private List<List<BaseUnit>> squadsList;

    public Squads currentSquad; // The squad whose turn it is

    private int currentSquadIndex = 0;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        squadsDict = new Dictionary<Squads, List<BaseUnit>>
        {
            { Squads.one, player1Squad },
            { Squads.two, player2Squad },
        };

        squadsList = new List<List<BaseUnit>> { player1Squad, player2Squad };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            EndTurn();
    }

    // Start the turn of the current unit
    public void StartTurn()
    {
        currentSquad = (Squads)(currentSquadIndex + 1);
        Campfire fire = GetCampfireOfSquad(currentSquad);
        fire.TryRevive();
        Debug.Log($"Switching Teams. Team {currentSquad}'s turn");
    }

    // End the current squad's turn and move to the next
    public void EndTurn()
    {
        currentSquadIndex = (currentSquadIndex + 1) % squadsList.Count; // Loop through the squads
        UnitManager.Instance.ResetTeam(squadsList[currentSquadIndex]);
        GridManager.Instance.Deselect();
        StartTurn();
    }

    public bool isUnitInCurrentSquad(BaseUnit unit)
    {
        if (unit.GetSquad == currentSquad) return true;
        return false;
    }

    public void AddUnitToSquad(BaseUnit unit, Squads team)
    {
        List<BaseUnit> squad = squadsDict[team];
        unit.SetSquad(team);
        squad.Add(unit);
    }

    public void RemoveUnitFromTurnSystem(BaseUnit unit)
    {
        foreach (var squad in squadsList)
        {
            if (squad.Contains(unit))
            {
                squad.Remove(unit);
                break;
            }
        }
    }

    public Campfire GetCampfireOfSquad(Squads squad)
    {
        foreach (var u in squadsDict[squad])
        {
            if (u is Campfire camp) return camp;
        }
        return null;
    }
}



