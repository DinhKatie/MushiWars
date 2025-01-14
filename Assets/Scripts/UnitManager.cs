using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;

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

    private Dictionary<Vector3Int, BaseUnit> _unitsOnTiles = new Dictionary<Vector3Int, BaseUnit>();
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

    public void LogUnitsOnTiles()
    {
        foreach (var entry in _unitsOnTiles)
        {
            Debug.Log($"Tile: {entry.Key}, Unit: {entry.Value}");
        }
    }

    public BaseUnit SpawnUnit(Vector3Int spawnTile, UnitPrefabs unitType, Squads squad)
    {
        // Check if the tile is valid and no unit is already there
        if (!GridManager.Instance.IsOccupied(spawnTile))
        {
            // Retrieve the prefab based on the enum type
            BaseUnit prefabToSpawn = unitPrefabsDict[unitType];
            
            Vector3 spawnPosition = _tilemap.GetCellCenterWorld(spawnTile);

            //Different rotation based on squad
            Quaternion spawnRotation = (int)squad % 2 == 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

            GameObject unitGO = PhotonNetwork.Instantiate(prefabToSpawn.name, spawnPosition, spawnRotation);
            BaseUnit newUnit = unitGO.GetComponent<BaseUnit>();

            newUnit.SetCurrentPosition(spawnTile);
            TurnManager.Instance.AddUnitToSquad(newUnit, squad);

            // Add the unit to the dictionary to track its position
            _unitsOnTiles[spawnTile] = newUnit;

            Debug.Log($"Unit spawned on tile {spawnTile}");
            newUnit.name = "Mushi " + _unitsOnTiles.Count;

            object[] unitData = newUnit.Serialize();
            PhotonView.Get(this).RPC("RPC_UpdateBoardState", RpcTarget.Others, spawnTile.x, spawnTile.y, spawnTile.z, unitData);

            return newUnit;
        }
        Debug.Log($"Tile {spawnTile} is either invalid or already has a unit.");
        return null;
    }

    [PunRPC]
    public void RPC_UpdateBoardState(int x, int y, int z, object[] unitData)
    {
        Debug.Log("Starting the Update Board RPC");

        // Deserialize the unit data
        BaseUnit newUnit = BaseUnit.CreateUnitFromData(unitData);

        // Rebuild the Vector3Int from the individual x, y, z integers
        Vector3Int spawnTileInt = new Vector3Int(x, y, z);

        // Update the unit's position
        newUnit.SetCurrentPosition(spawnTileInt);

        // Add the unit to the board state (assuming _unitsOnTiles is a dictionary)
        _unitsOnTiles[spawnTileInt] = newUnit;

        Debug.Log($"Unit updated on tile {spawnTileInt} via RPC.");
    }


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

    // Method to get the unit at a specific tiles
    public BaseUnit GetUnitAtTile(Vector3Int tilePosition)
    {
        _unitsOnTiles.TryGetValue(tilePosition, out BaseUnit unit);
        return unit;

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
        _unitsOnTiles[newPosition] = unit;

        unit.Move(newPosition);
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

    public void PushCampfire(BaseUnit pusher, Campfire campfire, Vector3Int tileToPush)
    {
        if (pusher.MovementRange <= 0) return; 

        Vector3Int pusherOldPos = pusher.CurrentPosition;
        Vector3Int fireOldPos = campfire.CurrentPosition;

        _unitsOnTiles.Remove(pusherOldPos);
        _unitsOnTiles.Remove(fireOldPos);

        pusher.SetCurrentPosition(campfire.CurrentPosition);
        campfire.SetCurrentPosition(tileToPush);

        _unitsOnTiles[campfire.CurrentPosition] = campfire;
        _unitsOnTiles[pusher.CurrentPosition] = pusher;

        pusher.DecrementMove();
        pusher.HighlightValidMoves();
    }

    public void UpdateUnitsAfterShrink()
    {
        List<BaseUnit> unitsToRemove = new List<BaseUnit>();
        foreach (KeyValuePair<Vector3Int, BaseUnit> entry in _unitsOnTiles)
        {
            Vector3Int tile = entry.Key;
            BaseUnit unit = entry.Value;

            if (GridManager.Instance.GetTileAtPosition(tile) == null)
            {
                unitsToRemove.Add(unit);
            }
        }

        foreach (BaseUnit unit in unitsToRemove)
        {
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
        _unitsOnTiles[newPosition] = unitToPort;

        unitToPort.Teleport(newPosition);
    }

    // --------------------------------

    // Update highlights when grid changes
    public void UpdateUnitHighlights()
    {
        foreach (var unit in _unitsOnTiles.Values)
        {
            if (unit != null)
                unit.HighlightValidMoves();
        }
    }

    public void GetUnitHighlights(BaseUnit unit)
    {
        Squads currSquad = TurnManager.Instance.GetCurrentSquad();
        //Campfire can't move on its own unless FireBoy is the hero
        //if (unit is Campfire && TurnManager.Instance.GetHeroType(currSquad) != HeroTypes.Fire) return; 
        unit.HighlightValidMoves();
    }

    public void ResetTeam(List<BaseUnit> squad)
    {
        foreach (var unit in squad)
        {
            unit.Reset();
        }
    }

    public void UseHeroAbility(List<BaseUnit> squad)
    {
        foreach( var unit in squad)
        {
            if (unit is BaseHero hero)
            {
                hero.UseAbility();
                break;
            }
        }
    }

    public List<Vector3Int> GetTeam(Squads squad)
    {
        List<Vector3Int> teamUnits = new List<Vector3Int>();

        foreach (var kvp in _unitsOnTiles)
        {
            if (kvp.Value.GetSquad == squad)
                teamUnits.Add(kvp.Value.CurrentPosition);
        }

        return teamUnits;
    }

    public static object[] SerializeDictionary(Dictionary<Vector3Int, BaseUnit> boardState)
    {
        Debug.Log("Seralizing Dictionary.");
        List<object> serializedData = new List<object>();

        foreach (var kvp in boardState)
        {
            int[] positionArray = new int[] { kvp.Key.x, kvp.Key.y, kvp.Key.z };
            object[] unitData = kvp.Value.Serialize();

            serializedData.Add(new object[] { positionArray, unitData });
        }

        return serializedData.ToArray();
    }

    public static Dictionary<Vector3Int, BaseUnit> DeserializeDictionary(object[] serializedData)
    {
        Debug.Log("Deserializing Dictionary");
        Dictionary<Vector3Int, BaseUnit> boardState = new Dictionary<Vector3Int, BaseUnit>();

        foreach (object entry in serializedData)
        {
            object[] keyValuePair = (object[])entry;

            // Deserialize position
            int[] posArray = (int[])keyValuePair[0];
            Vector3Int position = new Vector3Int(posArray[0], posArray[1], posArray[2]);

            // Deserialize unit
            BaseUnit unit = BaseUnit.CreateUnitFromData((object[])keyValuePair[1]);

            boardState.Add(position, unit);
        }

        return boardState;
    }

}





