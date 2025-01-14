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
        Debug.Log($"Switching Teams. Team {currentSquad}'s turn");


        // Check which Photon player owns the current squad
        if (squadOwners.TryGetValue(currentSquad, out Player owner))
        {
            Debug.Log(owner);
            if (owner == PhotonNetwork.LocalPlayer)
                EnablePlayerControls();
            else
                DisablePlayerControls();
        }
    }

    public void EnablePlayerControls()
    {
        _playerControlsEnabled = true;
        Debug.Log("Player controls enabled for local player.");
    }

    public void DisablePlayerControls()
    {
        _playerControlsEnabled = false;
        Debug.Log("Player controls disabled for local player.");
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
        if (PhotonNetwork.LocalPlayer == squadOwners[currentSquad])
        {
            Debug.Log("End Turn RPC Sent.");
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("EndTurnRPC", RpcTarget.All);
        }
    }

    [PunRPC]
    public void EndTurnRPC()
    {
        currentSquadIndex = (currentSquadIndex + 1) % squadsList.Count;
        UnitManager.Instance.ResetTeam(squadsList[currentSquadIndex]);
        GridManager.Instance.Deselect();
        StartTurn();
    }

    private void InitializeSquadOwners()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Assign the first two players to the two squads
        var players = PhotonNetwork.PlayerList;
        if (players.Length >= 2)
        {
            squadOwners[Squads.one] = players[0];
            squadOwners[Squads.two] = players[1];
        }

        // Copy squadOwners to a list to avoid modifying the dictionary during iteration
        var squadOwnersList = new List<KeyValuePair<Squads, Player>>(squadOwners);

        // Send the squadOwners to all clients
        PhotonView photonView = PhotonView.Get(this);
        foreach (var kvp in squadOwnersList)
        {
            photonView.RPC("SetSquadOwnerRPC", RpcTarget.AllBuffered, kvp.Key, kvp.Value.ActorNumber);
        }
        Debug.Log("Squad Owners Initialized and Sent.");
    }

    [PunRPC]
    public void SetSquadOwnerRPC(Squads squad, int ownerActorNumber)
    {
        Player owner = PhotonNetwork.CurrentRoom.GetPlayer(ownerActorNumber);
        if (owner != null)
        {
            squadOwners[squad] = owner;
        }
        else
        {
            Debug.LogError("Player not found with actor number: " + ownerActorNumber);
        }

    }

    public bool isUnitInCurrentSquad(BaseUnit unit)
    {
        if (unit.GetSquad == currentSquad) return true;
        return false;
    }

    public void AddUnitToSquad(BaseUnit unit, Squads team)
    {
        List<BaseUnit> squad = squadsDict[team];
        unit.SetSquad(team);
        squad.Add(unit);
    }

    public void RemoveUnitFromTurnSystem(BaseUnit unit)
    {
        List<BaseUnit> l = squadsDict[unit.GetSquad];
        if (l.Contains(unit))
            l.Remove(unit);
    }

    public Campfire GetCampfireOfSquad(Squads squad)
    {
        foreach (var u in squadsDict[squad])
        {
            if (u is Campfire camp) return camp;
        }
        return null;
    }

    public BaseHero GetHeroOfSquad(Squads squad)
    {
        foreach (var u in squadsDict[squad])
        {
            if (u is BaseHero hero) return hero;
        }
        return null;
    }

    public HeroTypes GetHeroType(Squads squad)
    {
        foreach (var unit in squadsDict[squad])
        {
            if (unit is BaseHero hero)
                return hero.heroType;
        }
        return HeroTypes.None;
    }

    public List<BaseUnit> GetAllUnitsExcept(Squads squad)
    {
        List<BaseUnit> otherSquads = new List<BaseUnit>();

        foreach(var s in squadsDict)
        {
            if (s.Key != squad)
            {
                otherSquads.AddRange(s.Value);
            }
        }

        return otherSquads;
    }
}



