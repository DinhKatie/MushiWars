using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseUnit : MonoBehaviour
{

    // Unit stats
    protected Vector3Int currPosition;
    protected int movementRange;
    protected int attackRange;
    protected bool turn;
    protected bool hasAttacked;

    // Getters
    public Vector3Int CurrentPosition => currPosition;
    public int MovementRange => movementRange;
    public int AttackRange => attackRange;
    public bool IsTurn => turn;
    public bool HasAttacked => hasAttacked;

    //Setters
    public void SetCurrentPosition(Vector3Int pos)
    { this.currPosition = pos; }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        movementRange = 2;
        attackRange = 1;
        hasAttacked = false;
    }

    public virtual void StartTurn()
    {
        Debug.Log($"It's Mushi on {currPosition}'s turn");
        //Reset Stats
        movementRange = 2;
        attackRange = 1;
        turn = true;
        hasAttacked = false;

        HighlightValidMoves();
    }

    public void EndTurn()
    {
        turn = false;
        GridManager.Instance.ClearValidMoves();
        GridManager.Instance.Deselect();
    }

    public void Move(Vector3Int newPosition)
    {
        int moveCost = CalculateMoveCost(newPosition);
        if (moveCost <= movementRange)
        {
            movementRange -= moveCost;
            currPosition = newPosition;
            transform.position = GridManager.Instance._tilemap.GetCellCenterWorld(newPosition);

            Debug.Log($"Unit Move Cost: {moveCost}");

            HighlightValidMoves();
        }
            
    }

    public void Attack(Vector3Int enemy)
    {
        UnitManager.Instance.RemoveUnit(enemy);
        HighlightValidMoves();
        GridManager.Instance.Deselect();
        hasAttacked = true;
    }

    public int CalculateMoveCost(Vector3Int newPosition)
    {
        int x = Mathf.Abs(currPosition.x - newPosition.x);
        int y = Mathf.Abs(currPosition.y - newPosition.y);

        return x + y;
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

    protected virtual List<Vector3Int> GetAttackRange()
    {
        List<Vector3Int> attackRanges = new List<Vector3Int>
        {
            currPosition + new Vector3Int(0, 1, 0),
            currPosition + new Vector3Int(0, -1, 0),
            currPosition + new Vector3Int(-1, 0, 0),
            currPosition + new Vector3Int(1, 0, 0)
        };
        return attackRanges;
    }

    public List<Vector3Int> CalculateValidAttacks()
    {
        List<Vector3Int> attackRanges = GetAttackRange();
        List<Vector3Int> toRemove = new List<Vector3Int>();
        foreach (var attack in attackRanges)
        {
            //If the tile doesn't have a unit on it, do not designate as an attackable tile (needa remove friendly fire later lmao)
            if (UnitManager.Instance.GetUnitAtTile(attack) == null)
                toRemove.Add(attack);
        }
        foreach (var attack in toRemove)
        {
            attackRanges.Remove(attack);
        }
        return attackRanges;
    }

    private void LogMoves(List<Vector3Int> validMoves)
    {
        foreach (var move in validMoves)
        {
            Debug.Log(move);
        }
    }

    protected void HighlightValidMoves()
    {
        GridManager.Instance.ClearValidMoves();

        List<Vector3Int> validMoves = CalculateValidMoves();
        List<Vector3Int> validAtt = CalculateValidAttacks();

        GridManager.Instance.HighlightValidMoves(validMoves);
        GridManager.Instance.HighlightValidAttacks(validAtt);
    }

}

