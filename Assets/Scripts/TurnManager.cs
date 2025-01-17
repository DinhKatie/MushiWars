using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public static TurnManager Instance;
    public List<BaseUnit> units;

    public List<BaseUnit> player1Squad;
    public List<BaseUnit> player2Squad;

    public Dictionary<Squads, List<BaseUnit>> squadsDict;
    private List<List<BaseUnit>> squadsList;

    // For Player Syncing
    private Dictionary<Squads, Player> squadOwners = new Dictionary<Squads, Player>(); //Connect players to their squads
    public bool _playerControlsEnabled;
    public bool _playerControlsOn => _playerControlsEnabled;

    private Squads currentSquad; // The squad whose turn it is

    private int currentSquadIndex = 0;

    public Squads GetCurrentSquad() => currentSquad;

    private int currentRound = 0;
    private int roundsPerShrink = 2;


    #region Initialization

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        squadsDict = new Dictionary<Squads, List<BaseUnit>>
        {
            { Squads.one, player1Squad },
            { Squads.two, player2Squad },
        };

        squadsList = new List<List<BaseUnit>> { player1Squad, player2Squad };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            EndTurn();
    }

    public void StartGame()
    { 
        if (PhotonNetwork.IsMasterClient)
        {
            InitializeSquadOwners();
            StartTurn();
        }
    }

    // Start the turn of the current unit
    public void StartTurn()
    {
        currentSquad = (Squads)(currentSquadIndex + 1);
        PhotonView.Get(this).RPC("StartTurnRPC", RpcTarget.All, (int)currentSquad);
        Debug.Log($"Switching Teams. Team {currentSquad}'s turn");

        // Check which Photon player owns the current squad
        if (squadOwners.TryGetValue(currentSquad, out Player owner))
            SetPlayerControls(owner == PhotonNetwork.LocalPlayer);
    }

    // End the current squad's turn and move to the next
    public void EndTurn()
    {
        /*if (currentSquadIndex == 0) //after the last player finishes their turn and we're back to player one
        {
            currentRound++;
            Debug.Log($"Current Round: {currentRound}");
            if (currentRound >= roundsPerShrink)
            {
                FindObjectOfType<ShrinkBoard>().BoardShrink();
                UnitManager.Instance.UpdateUnitsAfterShrink();
                currentRound = 0;
            }
        }*/
        if (isCurrentPlayer())
        {
            Debug.Log("End Turn RPC Sent.");
            PhotonView.Get(this).RPC("EndTurnRPC", RpcTarget.All);
        }
    }

    private void InitializeSquadOwners()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length && i < squadsDict.Count; i++)
        {
            squadOwners[(Squads)(i + 1)] = players[i];
            PhotonView.Get(this).RPC("SetSquadOwnerRPC", RpcTarget.AllBuffered, (int)(Squads)(i + 1), players[i].ActorNumber);
        }
        Debug.Log("Squad Owners Initialized and Sent.");
    }

    public void SetPlayerControls(bool enabled)
    {
        _playerControlsEnabled = enabled;
        Debug.Log($"Player controls {(enabled ? "enabled" : "disabled")} for local player.");
    }

    #endregion //--------------------------------------------------------------

    #region Remote Procedure Calls (RPCs)

    [PunRPC]
    public void StartTurnRPC(int currSquad)
    {
        currentSquad = (Squads)currSquad;
    }

    [PunRPC]
    public void EndTurnRPC()
    {
        currentSquadIndex = (currentSquadIndex + 1) % squadsList.Count;

        UnitManager.Instance.ResetTeam(squadsList[currentSquadIndex]);
        GridManager.Instance.Deselect();
        StartTurn();
    }


    [PunRPC]
    public void SetSquadOwnerRPC(Squads squad, int ownerActorNumber)
    {
        Player owner = PhotonNetwork.CurrentRoom.GetPlayer(ownerActorNumber);
        if (owner != null)
            squadOwners[squad] = owner;
        else
            Debug.LogError("Player not found with actor number: " + ownerActorNumber);

    }

    #endregion // ---------------------------------------------------------

    public void RemoveUnitFromTurnSystem(BaseUnit unit) => squadsDict[unit.GetSquad]?.Remove(unit);

    public bool isCurrentPlayer() => PhotonNetwork.LocalPlayer == squadOwners[currentSquad];

    public bool isUnitInCurrentSquad(BaseUnit unit) => unit.GetSquad == currentSquad;

    public void AddUnitToSquad(BaseUnit unit, Squads team)
    {
        unit.SetSquad(team);
        squadsDict[team].Add(unit);
    }


    public T GetUnitOfType<T>(Squads squad) where T : BaseUnit
    {
        foreach (var unit in squadsDict[squad])
        {
            if (unit is T typeUnit) return typeUnit;
        }
        return null;
    }

    public Campfire GetCampfireOfSquad(Squads squad) => GetUnitOfType<Campfire>(squad);

    public BaseHero GetHeroOfSquad(Squads squad) => GetUnitOfType<BaseHero>(squad);

    public HeroTypes GetHeroType(Squads squad) => GetHeroOfSquad(squad)?.heroType ?? HeroTypes.None;

    public List<BaseUnit> GetAllUnitsExcept(Squads squad)
    {
        List<BaseUnit> otherSquads = new List<BaseUnit>();

        foreach(var s in squadsDict)
        {
            if (s.Key != squad) //If the squad number is not equal to the given squad, add all its units to the list
                otherSquads.AddRange(s.Value);
        }

        return otherSquads;
    }
}



