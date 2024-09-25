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
        //HighlightValidMoves();
    }

    public void EndTurn()
    {
        turn = false;
    }

    public bool IsTurn()
    {
        return turn;
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}

