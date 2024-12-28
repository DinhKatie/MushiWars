using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardEffectInitializer : MonoBehaviour
{
    [SerializeField] private ScriptableCard hovercraft;
    [SerializeField] private ScriptableCard healthOrb;
    [SerializeField] private ScriptableCard blastStomp;
    [SerializeField] private ScriptableCard smite;
    [SerializeField] private ScriptableCard forcefield;
    [SerializeField] private ScriptableCard teleportation;
    [SerializeField] private ScriptableCard partyTime;
    [SerializeField] private ScriptableCard educate;
    [SerializeField] private ScriptableCard chillingWind;
    [SerializeField] private ScriptableCard curse;

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
    }

    private bool CantPlayCard() => HandManager.Instance.DisableCardEffects();

    private void Hovercraft()
    {
        if (CantPlayCard()) return;
        Debug.Log("Adding +1 move range to unit.");
        List<Vector3Int> validUnits = CurrentSquadUnits()
            .Select(tile => UnitManager.Instance.GetUnitAtTile(tile)) // Convert tiles to BaseUnits
            .Where(unit => unit != null && !unit.SkillsDisabled) // Filter non-null units with skills not disabled
            .Select(unit => unit.CurrentPosition)
            .ToList();

        StartCoroutine(SelectUnit(validUnits, unit => //Select a unit, then execute the following function with its return value
        {
            if (unit != null)
            {
                unit.IncrementMove();
                Debug.Log($"{unit} received 1 additional move range.");
            }
            GridManager.Instance.Deselect();
            GridManager.Instance.ClearValidMoves();
        }));
    }

    private void HealthOrb()
    {
        if (CantPlayCard()) return;

        if (!TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad()).UsesHexAndSupports)
        {
            Debug.Log("Hero cursed!");
            return;
        }

        Squads currSquad = TurnManager.Instance.GetCurrentSquad();
        BaseHero hero = TurnManager.Instance.GetHeroOfSquad(currSquad);
        hero?.IncrementHealth();
        Debug.Log($"Health of {hero} incremented by 1. New health: {hero.Health}");
    }

    private void BlastStomp()
    {
        if (CantPlayCard()) return;

        GridManager.Instance.avoidSelect = true;
        GridManager.Instance.Deselect();

        List<Vector3Int> validUnits = CurrentSquadUnits()
            .Select(tile => UnitManager.Instance.GetUnitAtTile(tile)) // Convert tiles to BaseUnits
            .Where(unit => unit != null && !unit.SkillsDisabled) // Filter non-null units with skills not disabled
            .Select(unit => unit.CurrentPosition)
            .ToList();

        StartCoroutine(SelectUnit(validUnits, unit =>
        {
            if (unit == null) return; //Invalid unit selection
            
            List<Vector3Int> validTiles = Utilities.GetValidTiles(unit, true);
            foreach (Vector3Int tile in validTiles) //Loop through each tile and check if an enemy unit exists on it
            {
                BaseUnit unitInRange = UnitManager.Instance.GetUnitAtTile(tile);
                if (unitInRange != null && !TurnManager.Instance.isUnitInCurrentSquad(unitInRange))
                {
                    unitInRange.TakeDamage();
                }
            }
            GridManager.Instance.Deselect();
            GridManager.Instance.avoidSelect = false;
            Debug.Log("Used Blast Stomp.");
        }));
       
    }

    private void Smite()
    {
        if (CantPlayCard()) return;

        GridManager.Instance.avoidSelect = true;
        GridManager.Instance.Deselect();

        List<Vector3Int> validUnits = CurrentSquadUnits()
            .Select(tile => UnitManager.Instance.GetUnitAtTile(tile)) // Convert tiles to BaseUnits
            .Where(unit => unit != null && !unit.SkillsDisabled) // Filter non-null units with skills not disabled
            .Select(unit => unit.CurrentPosition)
            .ToList();

        StartCoroutine(SelectUnit(validUnits, unit =>
        {
            if (unit == null) return; //Invalid unit selection

            List<Vector3Int> validTiles = Utilities.GetValidTiles(unit, true, 2);
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
                GridManager.Instance.avoidSelect = false; // Allow normal interaction if no enemies are available
                GridManager.Instance.Deselect();
                Debug.Log("No valid enemies to smite."); //CARD RETURN
                return;
            }

            StartCoroutine(SelectUnit(validEnemyUnits, enemy =>
            {
                if (enemy == null) return;

                enemy.AutoDie();
                Debug.Log($"Smited {enemy}");

                GridManager.Instance.Deselect();
                GridManager.Instance.avoidSelect = false;
            }));
        }));
    }

    private void Forcefield()
    {
        if (CantPlayCard()) return;

        List<Vector3Int> validUnits = CurrentSquadUnits()
            .Select(tile => UnitManager.Instance.GetUnitAtTile(tile)) // Convert tiles to BaseUnits
            .Where(unit => unit != null && !unit.SkillsDisabled) // Filter non-null units with skills not disabled
            .Select(unit => unit.CurrentPosition)
            .ToList();

        GridManager.Instance.avoidSelect = true;
        StartCoroutine(SelectUnit(validUnits, unit =>
        {
            unit?.SetImmune(true);
            Debug.Log($"Applied Forcefield");
            GridManager.Instance.avoidSelect = false;
            GridManager.Instance.Deselect();
        }));
    }

    private void Teleportation()
    {
        if (CantPlayCard()) return;

        List<Vector3Int> validUnits = CurrentSquadUnits()
            .Select(tile => UnitManager.Instance.GetUnitAtTile(tile)) // Convert tiles to BaseUnits
            .Where(unit => unit != null && !unit.SkillsDisabled) // Filter non-null units with skills not disabled
            .Select(unit => unit.CurrentPosition)
            .ToList();

        GridManager.Instance.avoidSelect = true;
        StartCoroutine(SelectUnit(validUnits, unit =>
        {
            if (!unit.HasNotActed())
            {
                Debug.Log("That unit has already acted this turn!"); //Teleportation can't be used if the unit has already acted in some way.
                GridManager.Instance.avoidSelect = false;
                GridManager.Instance.Deselect();
                return;
            }
            //Find all tiles in the list that are not occupied
            List<Vector3Int> validMoveTiles = Utilities.GetValidTiles(unit, true, 4).FindAll(tile => !GridManager.Instance.IsOccupied(tile));

            StartCoroutine(SelectTile(validMoveTiles, tile =>
            {
                UnitManager.Instance.TeleportUnit(unit, tile);
                unit.DisableMovementAndAttack();
                GridManager.Instance.avoidSelect = false;
                GridManager.Instance.Deselect();
            }));
        }));
    }

    private void PartyTime()
    {
        if (CantPlayCard()) return;

        List<Vector3Int> validUnits = CurrentSquadUnits()
            .Select(tile => UnitManager.Instance.GetUnitAtTile(tile)) // Convert tiles to BaseUnits
            .Where(unit => unit != null && !unit.SkillsDisabled) // Filter non-null units with skills not disabled
            .Select(unit => unit.CurrentPosition)
            .ToList();

        GridManager.Instance.avoidSelect = true;
        //Discard another card. Party Time cannot be played if no other cards are available to discard.
        if (HandManager.Instance.numCards() > 0)
        {
            StartCoroutine(WaitForDiscard(() =>
            {
                StartCoroutine(SelectUnit(validUnits, unit =>
                {
                    unit.Reset();
                    GridManager.Instance.avoidSelect = false;
                    GridManager.Instance.Deselect();
                }));
            }));
        } else
        {
            Debug.Log("No other cards to discard. Cannot play PartyTime.");
        }
    }

    private void Educate()
    {
        if (CantPlayCard()) return;

        if (TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad()).IsCursed)
        {
            Debug.Log("Hero cursed!");
            return;
        }

        int counter = 0;
        while (!HandManager.Instance.hasMaxHandSize() && counter < 2)
        {
            GetComponent<Deck>().DrawHand(1);
            counter++;
        }
    }

    private void ChillingWind()
    {
        if (CantPlayCard()) return;

        if (TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad()).IsCursed)
        {
            Debug.Log("Hero cursed!");
            return;
        }

        BaseHero currHero = TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad());
        List<Vector3Int> hexRange = GetHexRange(currHero, 4);
        List<Vector3Int> enemyUnits = EnemySquadUnits();
        List<Vector3Int> unitsWithinRange = enemyUnits.Where(unit => hexRange.Contains(unit)).ToList();

        GridManager.Instance.avoidSelect = true;
        StartCoroutine(SelectUnit(unitsWithinRange, unit =>
        {
            unit.SetChilled(true);
            Debug.Log($"Chilled {unit}");
            GridManager.Instance.avoidSelect = false;
            GridManager.Instance.Deselect();
        }));
    }

    private void Curse()
    {
        if (CantPlayCard()) return;

        if (TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad()).IsCursed)
        {
            Debug.Log("Hero cursed!");
            return;
        }

        BaseHero currHero = TurnManager.Instance.GetHeroOfSquad(TurnManager.Instance.GetCurrentSquad());
        List<Vector3Int> hexRange = GetHexRange(currHero, 4);
        List<Vector3Int> enemyUnits = EnemySquadUnits();
        List<Vector3Int> unitsWithinRange = enemyUnits.Where(unit => hexRange.Contains(unit)).ToList();

        GridManager.Instance.avoidSelect = true;

        StartCoroutine(SelectUnit(unitsWithinRange, unit =>
        {
            //Disable selecting his unit for skills. If hero, disable support/hexes
            unit.DisableSkills(true);
            if (EnemyHeroUnits().Contains(unit))
            {
                BaseHero hero = EnemyHeroUnits().FirstOrDefault(hero => hero == unit);
                hero.SetCursed(true);
            }

            StartCoroutine(SelectUnit(unitsWithinRange.Where(u => u != unit.CurrentPosition).ToList(), unit2 =>
            {
                //Disable selecting this second unit for skills. If hero, disable support/hexes
                unit2.DisableSkills(true);

                if (EnemyHeroUnits().Contains(unit))
                {
                    BaseHero hero = EnemyHeroUnits().FirstOrDefault(hero => hero == unit);
                    hero.SetCursed(true);
                }
                GridManager.Instance.avoidSelect = false;
                GridManager.Instance.Deselect();
            }));
        }));
    }

    private IEnumerator SelectUnit(List<Vector3Int> validTiles, Action<BaseUnit> onSelection)
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
            else if (Input.GetMouseButtonDown(1)) //Deselect
            {
                GridManager.Instance.Deselect();
                GridManager.Instance.avoidSelect = false;
                yield break;
            }
            yield return null;
        }

        onSelection?.Invoke(UnitManager.Instance.GetUnitAtTile(selectedTile)); //Pass the selected tile to the callback
    }

    private IEnumerator SelectTile(List<Vector3Int> validTiles, Action<Vector3Int> onSelection)
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
            else if (Input.GetMouseButtonDown(1)) //Deselect
            {
                GridManager.Instance.Deselect();
                GridManager.Instance.avoidSelect = false;
                yield break;
            }
            yield return null;
        }

        onSelection?.Invoke(selectedTile); //Pass the selected tile to the callback
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

    private List<Vector3Int> CurrentSquadUnits()
    {
        Squads currentSquad = TurnManager.Instance.GetCurrentSquad();
        List<Vector3Int> team = UnitManager.Instance.GetTeam(currentSquad);

        //Remove campfire from the list, since campfire can't be targeted by cards
        team.RemoveAll(tile =>
        {
            BaseUnit unit = UnitManager.Instance.GetUnitAtTile(tile);
            return unit != null && unit is Campfire;
        });

        return team;
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

    private List<Vector3Int> GetHexRange(BaseHero hero, int range)
    {
        return Utilities.GetValidTiles(hero, true, range).FindAll(tile => GridManager.Instance.GetTileAtPosition(tile) != null);
    }
}
