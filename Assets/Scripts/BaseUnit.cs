using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseUnit : MonoBehaviour
{
    //Unit stats
    public Vector3Int currPosition;
    public int movementRange = 2;

    //Unit turn information
    public bool turn;

    // Start is called before the first frame update
    void Start()
    {


    }

    public void StartTurn()
    {
        Debug.Log($"It's Mushi on {currPosition}'s turn");
        turn = true;
        List<Vector3Int> validMoves = CalculateValidMoves();
        Debug.Log("Calculating Valid Moves...");
        GridManager.Instance.HighlightValidMoves(validMoves);
        LogMoves(validMoves);
    }

    public void EndTurn()
    {
        turn = false;
        GridManager.Instance.ClearValidMoves();
    }

    public bool IsTurn()
    {
        return turn;
    }

    private List<Vector3Int> CalculateValidMoves()
    {
        List<Vector3Int> validMoves = new List<Vector3Int>();
        //Vector3Int startPos = GridManager.Instance._tilemap.WorldToCell(currPosition);
        Vector3Int startPos = currPosition;
        Debug.Log($"Calculating valid moves from position: {startPos}");
        //Loop through the valid move range.
        for (int x = -movementRange; x <= movementRange; x++)
        {
            for (int y = -movementRange; y <= movementRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > movementRange)
                    continue;

                Vector3Int tile = new Vector3Int(startPos.x + x, startPos.y + y, startPos.z);
                if (GridManager.Instance.GetTileAtPosition(tile) != null )
                {
                    validMoves.Add(tile);
                }
            }
        }
        return validMoves;
    }

    private void LogMoves(List<Vector3Int> validMoves)
    {
        foreach (var move in validMoves)
        {
            Debug.Log(move);
        }
    }


    // Update is called once per frame
    void Update()
    {
       
    }
}

