using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CardEffectInitializer : MonoBehaviour
{
    [SerializeField] private ScriptableCard hovercraft;
    [SerializeField] private ScriptableCard healthOrb;
    [SerializeField] private ScriptableCard blastStomp;

    private void Awake()
    {
        // Assign specific effects to ScriptableCards
        hovercraft.OnPlayEffect = () => Hovercraft();
        healthOrb.OnPlayEffect = () => HealthOrb();
        blastStomp.OnPlayEffect = () => BlastStomp();
    }

    private void Hovercraft()
    {
        Debug.Log("Adding +1 move range to unit.");
        List<Vector3Int> validUnits = CurrentSquadUnits();

        StartCoroutine(SelectUnit(validUnits, selectedTile => //Select a unit, then execute the following function with its return value
        {
            BaseUnit unit = UnitManager.Instance.GetUnitAtTile(selectedTile);
            if (unit != null)
            {
                unit.IncrementMove();
                Debug.Log($"Unit at {selectedTile} received 1 additional move range.");
            }
            GridManager.Instance.Deselect();
            GridManager.Instance.ClearValidMoves();
        }));
    }

    private void HealthOrb()
    {
        Squads currSquad = TurnManager.Instance.GetCurrentSquad();
        BaseHero hero = TurnManager.Instance.GetHeroOfSquad(currSquad);
        hero.IncrementHealth();
        Debug.Log($"Health of {hero} incremented by 1. New health: {hero.Health}");
    }

    private void BlastStomp()
    {
        List<Vector3Int> validUnits = CurrentSquadUnits();
        GridManager.Instance.avoidSelect = true;
        GridManager.Instance.Deselect();

        StartCoroutine(SelectUnit(validUnits, selectedTile =>
        {
            BaseUnit unit = UnitManager.Instance.GetUnitAtTile(selectedTile);
            if (unit == null) return; //Invalid unit selection
            
            List<Vector3Int> validTiles = GetValidTiles(unit, true);
            foreach (Vector3Int tile in validTiles) //Loop through each tile and check if an enemy unit exists on it
            {
                BaseUnit unitInRange = UnitManager.Instance.GetUnitAtTile(tile);
                if (unitInRange != null && !TurnManager.Instance.isUnitInCurrentSquad(unitInRange))
                {
                    unitInRange.TakeDamage();
                }
            }
            GridManager.Instance.Deselect();
            
        }));
       
    }

    private List<Vector3Int> GetValidTiles(BaseUnit unit, bool includesDiagonals, int range = 1)
    {
        Vector3Int currPosition = unit.CurrentPosition;
        List<Vector3Int> tiles = new List<Vector3Int>();

        //Orthogonals
        for (int i = 1; i <= range; i++)
        {
            tiles.Add(currPosition + Vector3Int.up * i);
            tiles.Add(currPosition + Vector3Int.down * i);
            tiles.Add(currPosition + Vector3Int.left * i);
            tiles.Add(currPosition + Vector3Int.right * i);
        }

        if (includesDiagonals)
        {
            for (int i = 1; i <= range; i++)
            {
                tiles.Add(currPosition + (Vector3Int.up + Vector3Int.left) * i);
                tiles.Add(currPosition + (Vector3Int.up + Vector3Int.right) * i);
                tiles.Add(currPosition + (Vector3Int.down + Vector3Int.left) * i);
                tiles.Add(currPosition + (Vector3Int.down + Vector3Int.right) * i);
            }
        }

        return tiles;
    }

    private IEnumerator SelectUnit(List<Vector3Int> validTiles, Action<Vector3Int> onSelection)
    {
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
                yield break;
            }
            yield return null;
        }

        onSelection?.Invoke(selectedTile); //Pass the selected tile to the callback
    }

    private List<Vector3Int> CurrentSquadUnits()
    {
        Squads currentSquad = TurnManager.Instance.GetCurrentSquad();
        return UnitManager.Instance.GetUnitByTeam(currentSquad);
    }
}
