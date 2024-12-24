using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CardEffectInitializer : MonoBehaviour
{
    [SerializeField] private ScriptableCard moveCard;

    private void Awake()
    {
        // Assign specific effects to ScriptableCards
        moveCard.OnPlayEffect = () => AddMove();
    }

    private void AddMove(int amount = 1)
    {
        Coroutine unitSelection;
        Debug.Log($"Adding {amount} of move range to unit.");
        Squads currentSquad = TurnManager.Instance.GetCurrentSquad();
        UnitManager.Instance.GetUnitByTeam(currentSquad);
        unitSelection = StartCoroutine(SelectUnit(UnitManager.Instance.GetUnitByTeam(currentSquad), amount));
    }

    private IEnumerator SelectUnit(List<Vector3Int> validTiles, int amount)
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
            else if (Input.GetMouseButtonDown(1)) //Deselect, but allow revival again.
            {
                GridManager.Instance.Deselect();
                yield break;
            }
            yield return null;
        }
        BaseUnit unit = UnitManager.Instance.GetUnitAtTile(selectedTile);
        if (unit != null)
        {
            unit.IncrementMove(amount);
            Debug.Log($"Unit at {selectedTile} received {amount} additional move range.");
        }
        GridManager.Instance.Deselect();
        GridManager.Instance.ClearValidMoves();
        yield return null;
    }
}
