using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class CardEffectInitializer : MonoBehaviour
{
    [SerializeField] private ScriptableCard hovercraft;
    [SerializeField] private ScriptableCard healthOrb;
    [SerializeField] private ScriptableCard blastStomp;
    [SerializeField] private ScriptableCard smite;
    [SerializeField] private ScriptableCard forcefield;

    private void Awake()
    {
        // Assign specific effects to ScriptableCards
        hovercraft.OnPlayEffect = () => Hovercraft();
        healthOrb.OnPlayEffect = () => HealthOrb();
        blastStomp.OnPlayEffect = () => BlastStomp();
        smite.OnPlayEffect = () => Smite();
        forcefield.OnPlayEffect = () => Forcefield();
    }

    private void Hovercraft()
    {
        Debug.Log("Adding +1 move range to unit.");
        List<Vector3Int> validUnits = CurrentSquadUnits();

        StartCoroutine(SelectUnit(validUnits, unit => //Select a unit, then execute the following function with its return value
        {
            if (unit != null)
            {
                unit.IncrementMove();
                Debug.Log($"{unit} received 1 additional move range.");
            }
            GridManager.Instance.Deselect();
            GridManager.Instance.ClearValidMoves();
        }));
    }

    private void HealthOrb()
    {
        Squads currSquad = TurnManager.Instance.GetCurrentSquad();
        BaseHero hero = TurnManager.Instance.GetHeroOfSquad(currSquad);
        hero?.IncrementHealth();
        Debug.Log($"Health of {hero} incremented by 1. New health: {hero.Health}");
    }

    private void BlastStomp()
    {
        List<Vector3Int> validUnits = CurrentSquadUnits();
        GridManager.Instance.avoidSelect = true;
        GridManager.Instance.Deselect();

        StartCoroutine(SelectUnit(validUnits, unit =>
        {
            if (unit == null) return; //Invalid unit selection
            
            List<Vector3Int> validTiles = Utilities.GetValidTiles(unit, true);
            foreach (Vector3Int tile in validTiles) //Loop through each tile and check if an enemy unit exists on it
            {
                BaseUnit unitInRange = UnitManager.Instance.GetUnitAtTile(tile);
                if (unitInRange != null && !TurnManager.Instance.isUnitInCurrentSquad(unitInRange))
                {
                    unitInRange.TakeDamage();
                }
            }
            GridManager.Instance.Deselect();
            GridManager.Instance.avoidSelect = false;
            Debug.Log("Used Blast Stomp.");
        }));
       
    }

    private void Smite()
    {
        List<Vector3Int> validUnits = CurrentSquadUnits();
        GridManager.Instance.avoidSelect = true;
        GridManager.Instance.Deselect();

        StartCoroutine(SelectUnit(validUnits, unit =>
        {
            if (unit == null) return; //Invalid unit selection

            List<Vector3Int> validTiles = Utilities.GetValidTiles(unit, true, 2);
            List<Vector3Int> validEnemyUnits = new List<Vector3Int>();

            foreach (Vector3Int tile in validTiles) //Loop through each tile and check if an enemy unit exists on it
            {
                BaseUnit unitInRange = UnitManager.Instance.GetUnitAtTile(tile);
                //Add valid enemy units for later selection (an enemy unit, is not a hero or campfire)
                if (unitInRange != null && !TurnManager.Instance.isUnitInCurrentSquad(unitInRange) &&
                    !(unitInRange is BaseHero || unitInRange is Campfire))
                    validEnemyUnits.Add(unitInRange.CurrentPosition);
            }

            if (validEnemyUnits.Count == 0)
            {
                GridManager.Instance.avoidSelect = false; // Allow normal interaction if no enemies are available
                GridManager.Instance.Deselect();
                Debug.Log("No valid enemies to smite.");
                return;
            }

            StartCoroutine(SelectUnit(validEnemyUnits, enemy =>
            {
                if (enemy == null) return;

                enemy.AutoDie();
                Debug.Log($"Smited {enemy}");

                GridManager.Instance.Deselect();
                GridManager.Instance.avoidSelect = false;
            }));
        }));
    }

    private void Forcefield()
    {
        GridManager.Instance.avoidSelect = true;
        List<Vector3Int> validUnits = CurrentSquadUnits();
        StartCoroutine(SelectUnit(validUnits, unit =>
        {
            unit?.SetImmune(true);
            Debug.Log($"Applied Forcefield");
            GridManager.Instance.avoidSelect = false;
            GridManager.Instance.Deselect();
        }));
    }

    private IEnumerator SelectUnit(List<Vector3Int> validTiles, Action<BaseUnit> onSelection)
    {
        GridManager.Instance.Deselect();
        GridManager.Instance.HighlightOutlineTiles(validTiles);
        Vector3Int selectedTile;

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3Int clickedTile = GridManager.Instance.GetMouseTilePosition();

                if (validTiles.Contains(clickedTile))
                {
                    selectedTile = clickedTile;
                    break;
                }
            }
            else if (Input.GetMouseButtonDown(1)) //Deselect
            {
                GridManager.Instance.Deselect();
                GridManager.Instance.avoidSelect = false;
                yield break;
            }
            yield return null;
        }

        onSelection?.Invoke(UnitManager.Instance.GetUnitAtTile(selectedTile)); //Pass the selected tile to the callback
    }

    private List<Vector3Int> CurrentSquadUnits()
    {
        Squads currentSquad = TurnManager.Instance.GetCurrentSquad();
        List<Vector3Int> team = UnitManager.Instance.GetTeam(currentSquad);

        //Remove campfire from the list, since campfire can't be targeted by cards
        team.RemoveAll(tile =>
        {
            BaseUnit unit = UnitManager.Instance.GetUnitAtTile(tile);
            return unit != null && unit is Campfire;
        });

        return team;
    }
}
