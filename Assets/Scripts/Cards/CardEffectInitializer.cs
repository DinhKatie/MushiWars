using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardEffectInitializer : MonoBehaviour
{

    [Header("Skills")]
    [SerializeField] private ScriptableCard hovercraft;
    [SerializeField] private ScriptableCard blastStomp;
    [SerializeField] private ScriptableCard smite;
    [SerializeField] private ScriptableCard forcefield;
    [SerializeField] private ScriptableCard teleportation;
    [SerializeField] private ScriptableCard partyTime;
    [SerializeField] private ScriptableCard navigation;

    [Header("Supports")]
    [SerializeField] private ScriptableCard healthOrb;
    [SerializeField] private ScriptableCard educate;

    [Header("Hexes")]
    [SerializeField] private ScriptableCard chillingWind;
    [SerializeField] private ScriptableCard curse;

    private CardEffectRPCs cardRPCs;
    private PhotonView photonView;

    private void Awake()
    {
        // Assign specific effects to ScriptableCards
        hovercraft.OnPlayEffect = () => Hovercraft();
        healthOrb.OnPlayEffect = () => HealthOrb();
        blastStomp.OnPlayEffect = () => BlastStomp();
        smite.OnPlayEffect = () => Smite();
        forcefield.OnPlayEffect = () => Forcefield();
        teleportation.OnPlayEffect = () => Teleportation();
        partyTime.OnPlayEffect = () => PartyTime();
        educate.OnPlayEffect = () => Educate();
        chillingWind.OnPlayEffect= () => ChillingWind();
        curse.OnPlayEffect = () => Curse();
        navigation.OnPlayEffect = () => Navigation();

        cardRPCs = GetComponent<CardEffectRPCs>();
        photonView = GetComponent<PhotonView>();
    }

    private bool CantPlayCard() => HandManager.Instance.DisableCardEffects();

    // ------------ SKILLS ----------------
    private void Hovercraft()
    {
        if (CantPlayCard()) return;
        Debug.Log("Adding +1 move range to unit.");

        StartCoroutine(Select(CurrentSquadUnits(), tile => UnitManager.Instance.GetUnitAtTile(tile), unit => //Select a unit, then execute the following function with its return value
        {
            if (unit != null)
            {
                unit.IncrementMove();
                Debug.Log($"{unit} received 1 additional move range.");
                photonView.RPC("HovercraftRPC", RpcTarget.Others, unit.GetComponent<PhotonView>().ViewID);
            }
            GridManager.Instance.Deselect();
            GridManager.Instance.ClearValidMoves();
        }));
    }

    private void BlastStomp()
    {
        if (CantPlayCard()) return;

        AvoidSelection(true);

        StartCoroutine(Select(CurrentSquadUnits(), tile => UnitManager.Instance.GetUnitAtTile(tile), unit =>
        {
            if (unit == null) return; //Invalid unit selection
            
            List<Vector3Int> validTiles = Utilities.GetValidTiles(unit, "square", 1);
            foreach (Vector3Int tile in validTiles) //Loop through each tile and check if an enemy unit exists on it
            {
                BaseUnit unitInRange = UnitManager.Instance.GetUnitAtTile(tile);
                if (unitInRange != null && !TurnManager.Instance.isUnitInCurrentSquad(unitInRange))
                {
                    unitInRange.TakeDamage();
                }
            }
            AvoidSelection(false);
            Debug.Log("Used Blast Stomp.");
        }));
       
    }

    private void Smite()
    {
        if (CantPlayCard()) return;

        AvoidSelection(true);

        StartCoroutine(Select(CurrentSquadUnits(), tile => UnitManager.Instance.GetUnitAtTile(tile), unit =>
        {
            if (unit == null) return; //Invalid unit selection

            List<Vector3Int> validTiles = Utilities.GetValidTiles(unit, "square", 2);
            List<Vector3Int> validEnemyUnits = new List<Vector3Int>();

            foreach (Vector3Int tile in validTiles) //Loop through each tile and check if an enemy unit exists on it
            {
                BaseUnit unitInRange = UnitManager.Instance.GetUnitAtTile(tile);
                //Add valid enemy units for later selection (an enemy unit, is not a hero or campfire)
                if (unitInRange != null && !TurnManager.Instance.isUnitInCurrentSquad(unitInRange) &&
                    !(unitInRange is BaseHero || unitInRange is Campfire))
                    validEnemyUnits.Add(unitInRange.CurrentPosition);
            }

            if (validEnemyUnits.Count == 0)
            {
                AvoidSelection(false);
                Debug.Log("No valid enemies to smite."); //CARD RETURN
                return;
            }

            StartCoroutine(Select(validEnemyUnits, tile => UnitManager.Instance.GetUnitAtTile(tile), enemy =>
            {
                if (enemy == null) return;

                enemy.AutoDie();
                Debug.Log($"Smited {enemy}");

                AvoidSelection(false);
            }));
        }));
    }

    private void Forcefield()
    {
        if (CantPlayCard()) return;

        AvoidSelection(true);
        StartCoroutine(Select(CurrentSquadUnits(), tile => UnitManager.Instance.GetUnitAtTile(tile), unit =>
        {
            unit?.SetImmune(true);
            photonView.RPC("ForcefieldRPC", RpcTarget.Others, unit.GetComponent<PhotonView>().ViewID);
            Debug.Log($"Applied Forcefield");
            AvoidSelection(false);
        }));
    }

    private void Teleportation()
    {
        if (CantPlayCard()) return;
        AvoidSelection(true);

        StartCoroutine(Select(CurrentSquadUnits(), tile => UnitManager.Instance.GetUnitAtTile(tile), unit =>
        {
            if (!unit.HasNotActed())
            {
                Debug.Log("That unit has already acted this turn!"); //Teleportation can't be used if the unit has already acted in some way.
                AvoidSelection(false); //CARD RETURN
                return;
            }
            //Find all tiles in the list that are not occupied
            List<Vector3Int> validMoveTiles = Utilities.GetValidTiles(unit, "square", 4).FindAll(tile => !GridManager.Instance.IsOccupied(tile));

            StartCoroutine(Select(validMoveTiles, tile => tile, tile =>
            {
                UnitManager.Instance.TeleportUnit(unit, tile);
                photonView.RPC("TeleportRPC", RpcTarget.Others, unit.GetComponent<PhotonView>().ViewID, tile.x, tile.y, tile.z);
                unit.DisableMovementAndAttack();
                AvoidSelection(false);
            }));
        }));
    }

    private void PartyTime()
    {
        if (CantPlayCard()) return;

        AvoidSelection(true);
        //Discard another card. Party Time cannot be played if no other cards are available to discard.
        if (HandManager.Instance.numCards > 0)
        {
            StartCoroutine(WaitForDiscard(() =>
            {
                StartCoroutine(Select(CurrentSquadUnits(), tile => UnitManager.Instance.GetUnitAtTile(tile), unit =>
                {
                    unit.Reset();
                    photonView.RPC("PartyTimeRPC", RpcTarget.Others, unit.GetComponent<PhotonView>().ViewID);
                    AvoidSelection(false);
                }));
            }));
        } else
        {
            Debug.Log("No other cards to discard. Cannot play PartyTime.");
        }
    }

    private void Navigation() //Clarify with Kevin
    {
        if (CantPlayCard()) return;

        List<Vector3Int> currentSquadNotActed = CurrentSquadUnits()
            .Where(tile => UnitManager.Instance.GetUnitAtTile(tile)?.HasNotActed() == true)
            .ToList();

        List<Vector3Int> actedUnits = CurrentSquadUnits()
            .Where(tile => UnitManager.Instance.GetUnitAtTile(tile)?.HasNotActed() == false)
            .ToList();
        if (actedUnits.Count == 0) { Debug.Log($"No Units have acted this turn."); return; }

        AvoidSelection(true);
        //Select a Mushi that has not acted
        StartCoroutine(Select(currentSquadNotActed, tile => UnitManager.Instance.GetUnitAtTile(tile), unit =>
        {
            Debug.Log($"{unit} selected as Navigator.");
            unit.DisableMovementAndAttack();

            //Select a Mushi that has already acted
            StartCoroutine(Select(actedUnits, tile => UnitManager.Instance.GetUnitAtTile(tile), unit2 =>
            {
                Debug.Log($"{unit2} selected by Navigation. Incrementing move by 3.");
                unit2.IncrementMove(3); //Up to 3? or exactly 3
                photonView.RPC("NavigationRPC", RpcTarget.Others, unit2.GetComponent<PhotonView>().ViewID);

                AvoidSelection(false);
            }));
        }));
    }

    // ------------ SUPPORTS ---------------
    private void HealthOrb()
    {
        if (!CardPlayable()) return;

        BaseHero hero = TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad());
        hero?.IncrementHealth();
        Debug.Log($"Health of {hero} incremented by 1. New health: {hero.Health}");
        photonView.RPC("HealthOrbRPC", RpcTarget.Others, hero.GetComponent<PhotonView>().ViewID);
    }

    private void Educate()
    {
        if (!CardPlayable()) return;

        int counter = 0;
        while (!HandManager.Instance.hasMaxHandSize() && counter < 2)
        {
            HandManager.Instance.DrawACard();
            counter++;
        }
    }

    // ------------ HEXES ---------------
    private void ChillingWind()
    {
        if (!CardPlayable()) return;

        BaseHero currHero = TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad());
        List<Vector3Int> unitsWithinRange = GetUnitsInHexRange(currHero, 4);

        AvoidSelection(true);
        StartCoroutine(Select(unitsWithinRange, tile => UnitManager.Instance.GetUnitAtTile(tile), unit =>
        {
            unit.SetChilled(true);
            Debug.Log($"Chilled {unit}");
            AvoidSelection(false);
        }));
    }

    private void Curse()
    {
        if (!CardPlayable()) return;

        BaseHero currHero = TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad());
        List<Vector3Int> unitsWithinRange = GetUnitsInHexRange(currHero, 4);

        AvoidSelection(true);

        StartCoroutine(Select(unitsWithinRange, tile => UnitManager.Instance.GetUnitAtTile(tile), unit =>
        {
            //Disable selecting his unit for skills. If hero, disable support/hexes
            unit.DisableSkills(true);
            if (EnemyHeroUnits().Contains(unit))
            {
                BaseHero hero = EnemyHeroUnits().FirstOrDefault(hero => hero == unit);
                hero.SetCursed(true);
            }

            StartCoroutine(Select(unitsWithinRange.Where(u => u != unit.CurrentPosition).ToList(), tile => UnitManager.Instance.GetUnitAtTile(tile), unit2 =>
            {
                //Disable selecting this second unit for skills. If hero, disable support/hexes
                unit2.DisableSkills(true);

                if (EnemyHeroUnits().Contains(unit))
                {
                    BaseHero hero = EnemyHeroUnits().FirstOrDefault(hero => hero == unit);
                    hero.SetCursed(true);
                }
                AvoidSelection(false);
            }));
        }));
    }


    // ------------ HELPERS ----------------

    private IEnumerator Select<T>(List<Vector3Int> validTiles, Func<Vector3Int, T> convertSelection, Action<T> onSelection)
    {
        GridManager.Instance.Deselect();
        GridManager.Instance.HighlightOutlineTiles(validTiles);
        Vector3Int selectedTile;

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3Int clickedTile = GridManager.Instance.GetMouseTilePosition();

                if (validTiles.Contains(clickedTile))
                {
                    selectedTile = clickedTile;
                    break;
                }
            }
            else if (Input.GetMouseButtonDown(1)) // Deselect
            {
                AvoidSelection(false);
                yield break;
            }
            yield return null;
        }

        onSelection?.Invoke(convertSelection(selectedTile));
    }

    private IEnumerator WaitForDiscard(Action action)
    {
        //Inform the user to discard a card (update UI)
        Debug.Log("Please discard a card.");

        bool cardDiscarded = false;
        Action discardListener = null;

        HandManager.Instance.discardingForCardEffect = true;
        discardListener = () =>
        {
            Debug.Log("Card discarded. Continuing effect...");
            cardDiscarded = true;

            action?.Invoke();

            GetComponent<Deck>().OnCardDiscarded -= discardListener;
        };

        GetComponent<Deck>().OnCardDiscarded += discardListener;

        // Wait until the card is discarded
        yield return new WaitUntil(() => cardDiscarded);
    }

    private bool CardPlayable()
    {
        return !CantPlayCard() && !TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad()).IsCursed;
    }

    private List<Vector3Int> CurrentSquadUnits()
    {
        Squads currentSquad = TurnManager.Instance.GetCurrentSquad();

        return UnitManager.Instance
            .GetTeam(currentSquad) //Get all tiles for the squad
            .Where(tile =>
            {
                BaseUnit unit = UnitManager.Instance.GetUnitAtTile(tile);
                return unit != null &&
                       !(unit is Campfire) && //Exclude Campfire units
                       !unit.SkillsDisabled;  //Exclude units with disabled skills
            })
            .Select(tile => UnitManager.Instance.GetUnitAtTile(tile).CurrentPosition) //Return a Vector3Int List
            .ToList();
    }

    private List<Vector3Int> EnemySquadUnits()
    {
        List<BaseUnit> otherTeams = TurnManager.Instance.GetAllUnitsExcept(TurnManager.Instance.GetCurrentSquad());
        List<Vector3Int> positions = otherTeams.Select(unit => unit.CurrentPosition).ToList();

        //Remove campfire from the list, since campfire can't be targeted by cards
        positions.RemoveAll(tile =>
        {
            BaseUnit unit = UnitManager.Instance.GetUnitAtTile(tile);
            return unit != null && unit is Campfire;
        });

        return positions;
    }

    private List<BaseHero> EnemyHeroUnits()
    {
        return EnemySquadUnits()
            .Select(pos => UnitManager.Instance.GetUnitAtTile(pos))
            .OfType<BaseHero>().ToList();
    }

    private void AvoidSelection(bool isAvoidingSelect)
    {
        GridManager.Instance.avoidSelect = isAvoidingSelect; // Allow normal interaction if no enemies are available
        GridManager.Instance.Deselect();
    }

    private List<Vector3Int> GetHexRange(BaseHero hero, int range)
    {
        return Utilities.GetValidTiles(hero, "square", range).FindAll(tile => GridManager.Instance.GetTileAtPosition(tile) != null);
    }

    private List<Vector3Int> GetUnitsInHexRange(BaseHero hero, int range)
    {
        List<Vector3Int> hexRange = GetHexRange(hero, range);
        List<Vector3Int> enemyUnits = EnemySquadUnits();
        return enemyUnits.Where(unit => hexRange.Contains(unit)).ToList();
    }
}
