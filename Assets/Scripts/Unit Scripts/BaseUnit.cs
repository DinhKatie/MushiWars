using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{

    // Unit stats
    protected Vector3Int currPosition;
    public int movementRange;
    protected int attackRange;
    protected bool hasAttacked;
    protected int health;
    protected bool dead = false;
    public bool justRevived = false;
    protected bool isImmune = false;
    protected bool isChilled = false;
    protected bool disabledSkills = false;

    public Squads squad;
    protected UnitPrefabs prefab = UnitPrefabs.unit;
    protected Animator unitAnim;
    [SerializeField] public AudioClip smite;

    // Getters
    public Vector3Int CurrentPosition => currPosition;
    public int MovementRange => movementRange;
    public int AttackRange => attackRange;
    public bool HasAttacked => hasAttacked;
    public int Health => health;
    public bool isDead => dead;
    public Squads GetSquad => squad;
    public bool Immune => isImmune;
    public bool Chilled => isChilled;
    public bool SkillsDisabled => disabledSkills;
    public Animator UnitAnimator => unitAnim;

    public UnitPrefabs GetPrefab => prefab;

    //Setters
    public void SetCurrentPosition(Vector3Int pos)
    {
        currPosition = pos;
        transform.position = Grid._tilemap.GetCellCenterWorld(pos);
    }
    public void SetSquad(Squads team) { squad = team; }
    public void DecrementMove(int moveCost = 1)
    {
        movementRange -= moveCost;
        Debug.Log("Movement Range is now " + movementRange);
    }
    public void IncrementMove(int move = 1)
    {
        movementRange += move;
        Debug.Log("Movement Range is now " + movementRange);
    }
    public void SetImmune(bool immune)
    {
        isImmune = immune;
    }
    public void SetChilled(bool chilled) {  isChilled = chilled; }
    public void DisableSkills(bool disable) { disabledSkills = disable; }

    //Managers for easy calling
    private GridManager Grid => GridManager.Instance;
    private UnitManager UnitMan => UnitManager.Instance;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        unitAnim = GetComponent<Animator>();
        health = 1;
        if (justRevived)
        {
            DisableMovementAndAttack();
            justRevived = false;
        }
        else
            ResetStats();

        PlayEffectAnimation("Smite");
    }

    protected virtual void ResetStats()
    {
        if (isChilled)
        {
            DisableMovementAndAttack();
            isChilled = false;
            return;
        }
        movementRange = 2;
        attackRange = 1;
        hasAttacked = false;
        isImmune = false;
    }

    public virtual void Reset() => ResetStats();

    // ------ MOVEMENT ------
    public virtual void Move(Vector3Int newPosition)
    {
        if (isChilled) { Debug.Log("Chilled!"); isChilled = false; return; }
        int moveCost = CalculateMoveCost(newPosition);
        if (moveCost <= movementRange)
        {
            movementRange -= moveCost;
            currPosition = newPosition;
            transform.position = Grid._tilemap.GetCellCenterWorld(newPosition);

            Debug.Log($"Unit Move Cost: {moveCost}");

            if (TurnManager.Instance.isCurrentPlayer())
                HighlightValidMoves();
        }
    }

    [PunRPC]
    public void RPC_MoveUnit(int x, int y, int z)
    {
        Debug.Log("Called RPC_MoveUnit");
        Vector3Int newPosition = new Vector3Int(x,y, z);
        UnitManager.Instance.UpdateUnitLocation(GetComponent<PhotonView>().ViewID, CurrentPosition, newPosition);
        Move(newPosition);
    }

    public int CalculateMoveCost(Vector3Int newPosition)
    {
        return Mathf.Abs(currPosition.x - newPosition.x) + Mathf.Abs(currPosition.y - newPosition.y);
    }

    private List<Vector3Int> CalculateValidMoves() //Breadth-first search
    {
        List<Vector3Int> validMoves = new List<Vector3Int>();
        Vector3Int startPos = currPosition;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);

        Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down };

        while (queue.Count > 0)
        {
            Vector3Int currentPos = queue.Dequeue();
            validMoves.Add(currentPos);

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighbor = currentPos + dir;

                // Check if the neighbor is within movement range and hasn't been visited yet.
                int distance = Mathf.Abs(neighbor.x - startPos.x) + Mathf.Abs(neighbor.y - startPos.y);
                if (distance > movementRange || visited.Contains(neighbor))
                    continue;

                // Check if the tile is valid (not an obstacle, no unit on it).
                if (!GridManager.Instance.IsOccupied(neighbor))
                {
                    // Mark the neighbor as visited and add it to the queue
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
        return validMoves;
    }


    // ------ ATTACKING -------
    public virtual void Attack(BaseUnit enemy)
    {
        if (isChilled) { Debug.Log("Chilled!"); isChilled = false; return; }
        if (enemy.isImmune)
        {
            Debug.Log("Enemy is Immune!");
            Grid.Deselect();
            return;
        }

        enemy.OnHit(1);
        hasAttacked = true;
        HighlightValidMoves();
        Grid.Deselect();
    }

    public List<Vector3Int> CalculateValidAttacks()
    {
        List<Vector3Int> attackRanges = GetAttackRange().Where(attack =>
        {
            BaseUnit unit = UnitMan.GetUnitAtTile(attack);
            return unit != null && !TurnManager.Instance.isUnitInCurrentSquad(unit); //Only attackable if the unit exists and is not in the current team
        }).ToList();

        return attackRanges;
    }

    protected virtual List<Vector3Int> GetAttackRange()
    {
        return Utilities.GetValidTiles(this, "orthogonal", attackRange);
    }

    // ----- CARD EFFECTS -------

    public void TakeDamage()
    {
        if (isImmune)
        {
            Debug.Log("Immune! (Take Damage)");
            return;
        }
        OnHit(1);
    }

    public void AutoDie()
    {
        if (isImmune)
        {
            Debug.Log("Immune! (AutoDie)");
            return;
        }
        GetComponent<PhotonView>().RPC("OnDeathRPC", RpcTarget.All);
    }

    public void Teleport(Vector3Int newPosition)
    {
        currPosition = newPosition;
        transform.position = Grid._tilemap.GetCellCenterWorld(newPosition);
    }

    public virtual bool HasNotActed()
    {
        return (movementRange == 2 && attackRange == 1 && hasAttacked == false);
    }


    // ----- ON HIT AND ON DEATH ------
    protected virtual void OnHit(int damage)
    {
        if (dead) return;

        Debug.Log("Applying Damage");
        health -= damage;
        Debug.Log($"{name} has been hit! Health: {health}");
        GetComponent<PhotonView>().RPC("OnHitRPC", RpcTarget.OthersBuffered, health);

        if (health <= 0)
        {
            GetComponent<PhotonView>().RPC("OnDeathRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    public void OnHitRPC(int newHealth)
    {
        Debug.Log($"New Health of {name}: {newHealth}");
        health = newHealth;
    }

    [PunRPC]
    public void OnDeathRPC()
    {
        Debug.Log("Calling OnDeath RPC");
        OnDeath();
    }

    public virtual void OnDeath()
    {
        UnitMan.RemoveUnit(currPosition);
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
        currPosition = new Vector3Int(-1, -1, -1);
        this.enabled = false;

        TurnManager.Instance.GetCampfireOfSquad(squad)?.RegisterDeadUnit(this);
    }

    public void DisableMovementAndAttack()
    {
        hasAttacked = true;
        movementRange = 0;
        attackRange = 0;
    }

    public virtual void HighlightValidMoves()
    {
        Grid.ClearValidMoves();

        Grid.HighlightValidMoves(CalculateValidMoves());

        //If unit hasn't attacked yet, highlight their valid attacks
        if (!hasAttacked)
            Grid.HighlightValidAttacks(CalculateValidAttacks());

        //Highlight if a campfire is pushable
        Grid.isCampfirePushable(this);
    }

    public void TriggerEffect(string effect)
    {
        Debug.Log($"{effect.ToUpper()} Effect");

        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No Animator found on this GameObject!");
            return;
        }

        Utilities.PlaySound($"{effect.ToLower()}", 0.3f);
    }

    public void PlayEffectAnimation(string effect)
    {
        //Get the effect gameObject
        Transform child = transform.Find($"{effect}");
        Debug.Log($"{effect} Effect");

        GameObject childEffect = null;
        if (child != null)
            childEffect = child.gameObject;

        Animator childAnimator = childEffect?.GetComponent<Animator>();

        childEffect.SetActive(true);
        childAnimator.Play(effect, -1, 0f);

        Utilities.PlaySound(effect.ToLower(), 0.3f);

        StartCoroutine(DisableAfterAnimation(childEffect, childAnimator));
    }

    private IEnumerator DisableAfterAnimation(GameObject effectObject, Animator animator)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        effectObject.SetActive(false);
    }
}

