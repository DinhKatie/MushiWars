using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;
using System.Linq;

public class UnitManager : MonoBehaviourPunCallbacks
{
    public static UnitManager Instance;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Tilemap _tilemap;

    [SerializeField] public BaseUnit unitPrefab;
    [SerializeField] public BaseUnit swordUnitPrefab;
    [SerializeField] public BaseUnit gunUnitPrefab;
    [SerializeField] public BaseUnit fireHeroPrefab;
    [SerializeField] public BaseUnit _campfirePrefab;

    private Dictionary<Vector3Int, int> _unitsOnTiles = new Dictionary<Vector3Int, int>();
    private Dictionary<UnitPrefabs, BaseUnit> unitPrefabsDict;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        unitPrefabsDict = new Dictionary<UnitPrefabs, BaseUnit>
        {
            { UnitPrefabs.unit, unitPrefab },
            { UnitPrefabs.swordUnit, swordUnitPrefab },
            { UnitPrefabs.gunUnit, gunUnitPrefab },
            { UnitPrefabs.fireHero, fireHeroPrefab },
            { UnitPrefabs.campfire, _campfirePrefab },
        };
    }

    #region Unit Spawning

    public void LogUnitsOnTiles()
    {
        foreach (var entry in _unitsOnTiles)
        {
            BaseUnit unit = GetUnitByViewID(entry.Value);
            Debug.Log($"Tile: {entry.Key}, Unit: {entry.Value} Squad: {unit.GetSquad} Type: {unit.GetType()}");
        }
    }

    public BaseUnit SpawnUnit(Vector3Int spawnTile, UnitPrefabs unitType, Squads squad)
    {
        if (GridManager.Instance.IsOccupied(spawnTile))
        {
            Debug.Log($"Tile {spawnTile} is either invalid or already has a unit.");
            return null;
        }

        BaseUnit prefabToSpawn = unitPrefabsDict[unitType];
        Vector3 spawnPosition = _tilemap.GetCellCenterWorld(spawnTile);
        Quaternion spawnRotation = (int)squad % 2 == 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;         //Different rotation based on squad

        GameObject unitGO = PhotonNetwork.Instantiate(prefabToSpawn.name, spawnPosition, spawnRotation);
        BaseUnit newUnit = unitGO.GetComponent<BaseUnit>();

        newUnit.SetCurrentPosition(spawnTile);
        TurnManager.Instance.AddUnitToSquad(newUnit, squad);

        int viewID = newUnit.GetComponent<PhotonView>().ViewID;
        _unitsOnTiles[spawnTile] = viewID;

        //Debug.Log($"Unit spawned on tile {spawnTile}");
        newUnit.name = "Mushi " + _unitsOnTiles.Count;

        PhotonView.Get(this).RPC("RPC_UpdateBoardState", RpcTarget.Others, spawnTile.x, spawnTile.y, spawnTile.z, viewID, squad);

        return newUnit;
        
    }

    #endregion

    #region Remote Procedure Calls (RPCs)
    [PunRPC]
    public void RPC_UpdateBoardState(int x, int y, int z, int viewID, int squad)
    {
        //Debug.Log("Starting the Update Board RPC");

        //rebuild the Vector3Int from the individual x, y, z integers
        Vector3Int spawnTileInt = new Vector3Int(x, y, z);

        //find the GameObject using the View ID
        BaseUnit newUnit = GetUnitByViewID(viewID);

        newUnit.SetCurrentPosition(spawnTileInt);
        TurnManager.Instance.AddUnitToSquad(newUnit, (Squads)squad);
        _unitsOnTiles[spawnTileInt] = viewID;

        //LogUnitsOnTiles();

    }

    [PunRPC]
    public void PushCampfireRPC(int pusherViewID, int campfireViewID, int x, int y, int z)
    {
        BaseUnit pusher = GetUnitByViewID(pusherViewID);
        BaseUnit campfire = GetUnitByViewID(campfireViewID);

        _unitsOnTiles.Remove(pusher.CurrentPosition);
        _unitsOnTiles.Remove(campfire.CurrentPosition);

        pusher.SetCurrentPosition(campfire.CurrentPosition);
        campfire.SetCurrentPosition(new Vector3Int(x,y,z));

        _unitsOnTiles[campfire.CurrentPosition] = campfire.GetComponent<PhotonView>().ViewID;
        _unitsOnTiles[pusher.CurrentPosition] = pusher.GetComponent<PhotonView>().ViewID;

    }

    #endregion

    #region Unit Management

    public void RemoveUnit(Vector3Int unitTile)
    {
        BaseUnit unit = GetUnitAtTile(unitTile);
        if (unit != null)
        {
            _unitsOnTiles.Remove(unit.CurrentPosition);
            //Remove the killed unit from the turn system
            TurnManager.Instance.RemoveUnitFromTurnSystem(unit);
        }
    }

    public void MoveUnit(BaseUnit unit, Vector3Int newPosition)
    {
        if (GetUnitAtTile(newPosition) != null || unit.MovementRange <= 0 || unit.CalculateMoveCost(newPosition) > unit.MovementRange) return;
        if (GridManager.Instance.IsObstacleTile(newPosition))
        {
            Debug.Log("That is an obstacle.");
            return;
        }

        _unitsOnTiles.Remove(unit.CurrentPosition);
        _unitsOnTiles[newPosition] = unit.GetComponent<PhotonView>().ViewID;

        unit.GetComponent<PhotonView>().RPC("RPC_MoveUnit", RpcTarget.All, newPosition.x, newPosition.y, newPosition.z);
    }

    public void AttackUnit(BaseUnit attacker, BaseUnit hitUnit)
    {
        //One attack per turn
        if (attacker.HasAttacked)
        {
            Debug.Log($"{attacker.name} has attacked already!");
            return;
        }

        //Ensure target is within the current unit's attack range
        List<Vector3Int> attackRanges = attacker.CalculateValidAttacks();
        if (attackRanges.Contains(hitUnit.CurrentPosition) && GetUnitAtTile(hitUnit.CurrentPosition))
        {
            attacker.Attack(hitUnit);
        }
        else
            Debug.Log($"{hitUnit} is out of attack range");
    }

    #endregion


    public void UpdateUnitLocation(int viewID, Vector3Int oldPosition, Vector3Int newPosition)
    {
        if (GridManager.Instance.GetTileAtPosition(newPosition))
        {
            _unitsOnTiles[newPosition] = viewID;
            _unitsOnTiles.Remove(oldPosition);
        }
    }

    public BaseUnit GetUnitAtTile(Vector3Int tilePosition)
    {
        return _unitsOnTiles.TryGetValue(tilePosition, out int viewID) //ternary
            ? GetUnitByViewID(viewID)
            : null;
    }

    public void PushCampfire(BaseUnit pusher, Campfire campfire, Vector3Int tileToPush)
    {
        if (pusher.MovementRange <= 0) return;
        int pusherViewID = pusher.GetComponent<PhotonView>().ViewID;
        int campfireViewID = campfire.GetComponent<PhotonView>().ViewID;

        GetComponent<PhotonView>().RPC("PushCampfireRPC", RpcTarget.AllBuffered, pusherViewID, campfireViewID, tileToPush.x, tileToPush.y, tileToPush.z);
        pusher.DecrementMove();
        pusher.HighlightValidMoves();
    }

    public void UpdateUnitsAfterShrink()
    {
        List<int> unitsToRemove = new List<int>();

        unitsToRemove = _unitsOnTiles
        .Where(entry => GridManager.Instance.GetTileAtPosition(entry.Key) == null)
        .Select(entry => entry.Value)  //Get the unit's viewID
        .ToList();

        foreach (int viewID in unitsToRemove)
        {
            BaseUnit unit = GetUnitByViewID(viewID);

            unit.OnDeath();
            RemoveUnit(unit.CurrentPosition);
        }
    }

    // ----- CARD EFFECTS ------

    public void TeleportUnit(BaseUnit unitToPort, Vector3Int newPosition)
    {
        if (GridManager.Instance.IsObstacleTile(newPosition) || GetUnitAtTile(newPosition) != null)
        {
            Debug.Log("There is an obstacle or unit on this square.");
            return;
        }

        _unitsOnTiles.Remove(unitToPort.CurrentPosition);
        _unitsOnTiles[newPosition] = unitToPort.GetComponent<PhotonView>().ViewID;

        unitToPort.Teleport(newPosition);
    }

    // --------------------------------

    public void ResetTeam(List<BaseUnit> squad) => squad.ForEach(unit => unit.Reset());

    public void UseHeroAbility(List<BaseUnit> squad) => squad.OfType<BaseHero>().FirstOrDefault()?.UseAbility();

    public BaseUnit GetUnitByViewID(int viewID) => PhotonView.Find(viewID).gameObject.GetComponent<BaseUnit>();

    // Update highlights when grid changes
    public void UpdateUnitHighlights()
    {
        foreach (var viewID in _unitsOnTiles.Values)
        {
            BaseUnit unit = GetUnitByViewID(viewID);
            unit?.HighlightValidMoves();
        }
    }

    public void GetUnitHighlights(BaseUnit unit)
    {
        Squads currSquad = TurnManager.Instance.GetCurrentSquad();
        //Campfire can't move on its own unless FireBoy is the hero
        //if (unit is Campfire && TurnManager.Instance.GetHeroType(currSquad) != HeroTypes.Fire) return; 
        unit.HighlightValidMoves();
    }


    public List<Vector3Int> GetTeam(Squads squad)
    {
        List<Vector3Int> teamUnits = new List<Vector3Int>();

        foreach (var kvp in _unitsOnTiles)
        {
            BaseUnit unit = GetUnitByViewID(kvp.Value);
            if (unit.GetSquad == squad)
                teamUnits.Add(unit.CurrentPosition);
        }

        return teamUnits;
    }

}





