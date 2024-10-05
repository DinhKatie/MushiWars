using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Campfire : BaseUnit
{
    public bool canRevive; //One unit revival per turn
    public List<BaseUnit> graveyard = new List<BaseUnit>(); //Keep track of dead units
    Coroutine revivalSelection;

    private void Update()
    {
        if (revivalSelection == null && canRevive && graveyard.Count > 0 && TurnManager.Instance.GetCurrentSquad() == squad)
        {
            TryRevive();
        }
    }
    protected override void ResetStats()
    {
        movementRange = 0;
        attackRange = 0;
        hasAttacked = false;
        canRevive = true;
    }

    public void RegisterDeadUnit(BaseUnit unit) => graveyard.Add(unit);

    public void UnregisterDeadUnit(BaseUnit unit) => graveyard.Remove(unit);

    private void ListGraveyard()
    {
        Debug.Log("Pick the corresponding key to revive a character: \n");
        for (int i = 0; i < graveyard.Count; i++)
        {
            string className = graveyard[i].GetType().Name;
            Debug.Log($"{i}: {className}\n");
        }
    }

    private void TryRevive()
    {
        if (!canRevive || graveyard.Count == 0) return;
        
        ListGraveyard();

        if (revivalSelection == null)
            revivalSelection = StartCoroutine(WaitForReviveSelection());
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
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void ReviveUnitFromGraveyard(int index)
    {
        if (index >= 0 && index < graveyard.Count && squad == TurnManager.Instance.GetCurrentSquad())
        {
            BaseUnit unitToRevive = graveyard[index];

            List<Vector3Int> validTiles = GetValidRevivalTiles(CurrentPosition);
            GridManager.Instance.HighlightRevivalTiles(validTiles);

            StartCoroutine(WaitForTileSelection(unitToRevive, validTiles));
        }
    }

    // Coroutine to wait for the player to select a tile
    private IEnumerator WaitForTileSelection(BaseUnit unitToRevive, List<Vector3Int> validTiles)
    {
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
            else if (Input.GetMouseButtonDown(1)) //Deselect, but allow revival again.
            {
                GridManager.Instance.Deselect();
                canRevive = true;
                revivalSelection = null;
                yield break;
            }

            yield return null;
        }

        // Spawn the unit on the selected tile
        BaseUnit newUnit = UnitManager.Instance.SpawnUnit(selectedTile, unitToRevive.GetPrefab, squad);
        newUnit.justRevived = true;

        GridManager.Instance.Deselect();
        graveyard.Remove(unitToRevive);
        Destroy(unitToRevive.gameObject);
        canRevive = false;
        revivalSelection = null;
    }

  // Get the valid cardinal direction tiles around the campfire
    private List<Vector3Int> GetValidRevivalTiles(Vector3Int campfirePosition)
    {
        List<Vector3Int> validTiles = new List<Vector3Int>
        {
            campfirePosition + Vector3Int.up,
            campfirePosition + Vector3Int.down,
            campfirePosition + Vector3Int.right,
            campfirePosition + Vector3Int.left
        };

        // Do not spawn on invalid tiles
        validTiles = validTiles.Where(tile =>
            GridManager.Instance.GetTileAtPosition(tile) &&
            UnitManager.Instance.GetUnitAtTile(tile) == null &&
            !GridManager.Instance.IsObstacleTile(tile)
        ).ToList();

        return validTiles;
    }

    //Campfires cannot move or attack
    public override void Move(Vector3Int newPosition) { return; }

    public override void Attack(BaseUnit enemy) { return; }

    protected override void OnDeath()
    {
        graveyard = null;
        StopAllCoroutines();
        UnitManager.Instance.RemoveUnit(currPosition);
        Destroy(gameObject);
    }
}

