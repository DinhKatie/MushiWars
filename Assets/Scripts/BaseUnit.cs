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
        GridManager.Instance.HighlightValidMoves(validMoves);
    }

    public void Move(Vector3Int newPosition)
    {
        int moveCost = CalculateMoveCost(newPosition);
        movementRange -= moveCost;
        currPosition = newPosition;
        transform.position = GridManager.Instance._tilemap.GetCellCenterWorld(newPosition);

        Debug.Log($"Unit Move Cost: {moveCost}");

        GridManager.Instance.ClearValidMoves();
        List<Vector3Int> validMoves = CalculateValidMoves();
        GridManager.Instance.HighlightValidMoves(validMoves);
    }

    public int CalculateMoveCost(Vector3Int newPosition)
    {
        int x = Mathf.Abs(currPosition.x - newPosition.x);
        int y = Mathf.Abs(currPosition.y - newPosition.y);

        return x + y;
    }


    public void EndTurn()
    {
        turn = false;
        GridManager.Instance.ClearValidMoves();
    }

    public bool IsTurn() => turn;

    public int GetMoveRange() => movementRange;

    public int GetAttackRange() => attackRange;

    private List<Vector3Int> CalculateValidMoves()
    {
        List<Vector3Int> validMoves = new List<Vector3Int>();
        Vector3Int startPos = currPosition;
        //Loop through the valid move range.
        for (int x = -movementRange; x <= movementRange; x++)
        {
            for (int y = -movementRange; y <= movementRange; y++)
            {
                //A diagonal move is 2 moves. Account for this.
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

