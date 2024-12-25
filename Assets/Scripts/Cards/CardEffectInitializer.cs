using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CardEffectInitializer : MonoBehaviour
{
    [SerializeField] private ScriptableCard hovercraft;

    private void Awake()
    {
        // Assign specific effects to ScriptableCards
        hovercraft.OnPlayEffect = () => Hovercraft();
    }

    private void Hovercraft()
    {
        Debug.Log("Adding +1 move range to unit.");
        Squads currentSquad = TurnManager.Instance.GetCurrentSquad();
        List<Vector3Int> validTiles = UnitManager.Instance.GetUnitByTeam(currentSquad);

        StartCoroutine(SelectUnit(validTiles, selectedTile => //Select a unit, then execute the following function with its return value
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
}
