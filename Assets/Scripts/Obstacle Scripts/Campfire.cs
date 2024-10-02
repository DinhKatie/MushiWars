using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        Debug.Log("Pick the corresponding key to revive a character: \n");
        for (int i = 0; i < graveyard.Count; i++)
        {
            string className = graveyard[i].GetType().Name;
            Debug.Log($"{i}: {className}\n");
        }
        StartCoroutine(WaitForReviveSelection());
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

    private IEnumerator WaitForReviveSelection()
    {
        while (true)
        {
            for (int i = 0; i < graveyard.Count && i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i)) // Press '0' for the first, '1' for the second, etc.
                {
                    Debug.Log($"Player selected {graveyard[i].GetType().Name} to revive.");
                    ReviveUnitFromGraveyard(i);
                    break;
                }
            }

            yield return null;
        }
    }

    public void ReviveUnitFromGraveyard(int index)
    {
        if (index >= 0 && index < graveyard.Count)
        {
            BaseUnit unitToRevive = graveyard[index];
            Campfire campfire = TurnManager.Instance.GetCampfireOfSquad(unitToRevive.GetSquad);
            if (campfire != null)
            {
                List<Vector3Int> validTiles = GetValidRevivalTiles(campfire.CurrentPosition);
                HighlightValidRevivalTiles(validTiles);

                StartCoroutine(WaitForTileSelection(unitToRevive, validTiles));
                graveyard.Remove(unitToRevive);
            }
        }
        else
        {
            Debug.Log("Invalid selection. Please choose a valid unit.");
        }
    }

    // Get the valid cardinal direction tiles around the campfire
    private List<Vector3Int> GetValidRevivalTiles(Vector3Int campfirePosition)
    {
        List<Vector3Int> validTiles = new List<Vector3Int>
        {
            campfirePosition + new Vector3Int(0, 1, 0),
            campfirePosition + new Vector3Int(0, -1, 0),
            campfirePosition + new Vector3Int(1, 0, 0),
            campfirePosition + new Vector3Int(-1, 0, 0)
        };

        // Do not spawn on invalid tiles
        validTiles = validTiles.Where(tile =>
            GridManager.Instance.GetTileAtPosition(tile) &&
            UnitManager.Instance.GetUnitAtTile(tile) == null &&
            !GridManager.Instance.IsObstacleTile(tile)
        ).ToList();

        return validTiles;
    }

    // Highlight the valid revival tiles
    private void HighlightValidRevivalTiles(List<Vector3Int> validTiles)
    {
        GridManager.Instance.HighlightRevivalTiles(validTiles);
    }

    // Coroutine to wait for the player to select a tile
    private IEnumerator WaitForTileSelection(BaseUnit unitToRevive, List<Vector3Int> validTiles)
    {
        bool tileSelected = false;
        Vector3Int selectedTile = new Vector3Int(-1, -1, -1);

        while (!tileSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3Int clickedTile = GridManager.Instance.GetMouseTilePosition();

                if (validTiles.Contains(clickedTile))
                {
                    selectedTile = clickedTile;
                    tileSelected = true;
                }
            } 
            else if (Input.GetMouseButtonDown(1)) 
            {
                GridManager.Instance.Deselect();
                yield break;
            }

            yield return null; // Wait for the next frame
        }

        // Spawn the unit on the selected tile
        UnitManager.Instance.SpawnUnit(selectedTile, unitToRevive.GetPrefab, squad);
        UnregisterDeadUnit(unitToRevive);
        GridManager.Instance.Deselect(); // Clear the highlights
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

