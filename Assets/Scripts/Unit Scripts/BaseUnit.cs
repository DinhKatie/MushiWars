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
    protected bool hasAttacked;
    protected int health;
    protected bool dead = false;

    public Squads squad;

    // Getters
    public Vector3Int CurrentPosition => currPosition;
    public int MovementRange => movementRange;
    public int AttackRange => attackRange;
    public bool HasAttacked => hasAttacked;
    public int Health => health;
    public bool isDead => dead;
    public Squads GetSquad { get { return squad; } }

    //Setters
    public void SetCurrentPosition(Vector3Int pos) { currPosition = pos; }
    public void SetSquad(Squads team) { squad = team; }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = 1;
        ResetStats();
    }

    protected virtual void ResetStats()
    {
        movementRange = 2;
        attackRange = 1;
        hasAttacked = false;
    }

    public virtual void Move(Vector3Int newPosition)
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

    public virtual void Attack(BaseUnit enemy)
    {
        enemy.OnHit();
        hasAttacked = true;
        HighlightValidMoves();
        GridManager.Instance.Deselect();
    }

    protected virtual void OnHit()
    {
        health -= 1;
        Debug.Log($"{name} has been hit! Health: {health}");
        if (health <= 0)
        {
            UnitManager.Instance.RemoveUnit(currPosition);
            OnDeath();
        }   
    }

    protected void OnDeath()
    {
        dead = true;
        GetComponent<SpriteRenderer>().enabled = false;

        // Iterate through all child objects and disable their SpriteRenderer components (sword, gun, etc)
        foreach (Transform child in transform)
        {
            SpriteRenderer childSprite = child.GetComponent<SpriteRenderer>();
            if (childSprite != null)
                childSprite.enabled = false;
        }
        //Simulate death and disable
        currPosition = new Vector3Int(-1,-1,-1);
        this.enabled = false;
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

                //If the tile is valid, there's no unit, and it's not an obstacle: It's a valid move.
                if (GridManager.Instance.GetTileAtPosition(tile) != null && UnitManager.Instance.GetUnitAtTile(tile) == null
                    && !GridManager.Instance.IsObstacleTile(tile))
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
            BaseUnit unit = UnitManager.Instance.GetUnitAtTile(attack);

            //Do not designate as attackable tile if the tile is empty or if it is a unit within the team
            if (unit == null || TurnManager.Instance.isUnitInCurrentSquad(unit))
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

    public virtual void HighlightValidMoves()
    {
        GridManager.Instance.ClearValidMoves();
        
        List<Vector3Int> validMoves = CalculateValidMoves();
        GridManager.Instance.HighlightValidMoves(validMoves);

        //If unit hasn't attacked yet, highlight their valid attacks
        if (!hasAttacked)
        {
            List<Vector3Int> validAtt = CalculateValidAttacks();
            GridManager.Instance.HighlightValidAttacks(validAtt);
        }

        //Highlight if a campfire is pushable
        GridManager.Instance.isCampfirePushable(this);

    }

    public void Reset()
    {
        ResetStats();
    }

}

