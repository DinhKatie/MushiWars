using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseUnit : MonoBehaviour
{
    //Unit stats
    public Vector3Int currPosition;
    public int movementRange;
    public int attackRange;

    //Unit turn information
    public bool turn;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void StartTurn()
    {
        Debug.Log($"It's Mushi on {currPosition}'s turn");
        movementRange = 2;
        turn = true;
        List<Vector3Int> validMoves = CalculateValidMoves();
        Debug.Log("Calculating Valid Moves...");
        GridManager.Instance.HighlightValidMoves(validMoves);
        LogMoves(validMoves);
    }

    public void Move(Vector3Int newPosition)
    {
        int x = Mathf.Abs(currPosition.x - newPosition.x);
        int y = Mathf.Abs(currPosition.y - newPosition.y);

        Debug.Log($"Curr Position = {currPosition}, New Position = {newPosition}, x = {x}, y = {y}");

        int moveCost = x + y;
        movementRange -= moveCost;
        currPosition = newPosition;

        Debug.Log($"Unit Movement Range: {movementRange}");

        GridManager.Instance.ClearValidMoves();
        List<Vector3Int> validMoves = CalculateValidMoves();
        GridManager.Instance.HighlightValidMoves(validMoves);
    }


    public void EndTurn()
    {
        turn = false;
        GridManager.Instance.ClearValidMoves();
    }

    public bool IsTurn() => turn;

    public int GetMoveRange() => movementRange;

    public int GetAttackRange() => attackRange;


    public int CalculateMoveCost(Vector3Int newPosition)
    {
        int x = Mathf.Abs(currPosition.x - newPosition.x);
        int y = Mathf.Abs(currPosition.y - newPosition.y);

        Debug.Log($"Curr Position = {currPosition}, New Position = {newPosition}, x = {x}, y = {y}");

        int moveCost = x + y;

        return moveCost;
    }

    private List<Vector3Int> CalculateValidMoves()
    {
        List<Vector3Int> validMoves = new List<Vector3Int>();
        Vector3Int startPos = currPosition;
        //Loop through the valid move range.
        for (int x = -movementRange; x <= movementRange; x++)
        {
            for (int y = -movementRange; y <= movementRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) > movementRange)
                    continue;

                Vector3Int tile = new Vector3Int(startPos.x + x, startPos.y + y, startPos.z);
                if (GridManager.Instance.GetTileAtPosition(tile) != null && UnitManager.Instance.GetUnitAtTile(tile) == null)
                {
                    validMoves.Add(tile);
                }
            }
        }
        validMoves.Add(startPos);
        return validMoves;
    }

    private void LogMoves(List<Vector3Int> validMoves)
    {
        foreach (var move in validMoves)
        {
            Debug.Log(move);
        }
    }

    private void HighlightValidMoves()
    {
        GridManager.Instance.ClearValidMoves();
        List<Vector3Int> validMoves = CalculateValidMoves();
        GridManager.Instance.HighlightValidMoves(validMoves);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}

