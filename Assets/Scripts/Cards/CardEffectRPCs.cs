using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardEffectRPCs : MonoBehaviour
{
    [PunRPC]
    public void HovercraftRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit?.IncrementMove();
    }

    [PunRPC]
    public void HealthOrbRPC(int heroViewID)
    {
        BaseHero hero = (BaseHero) Utilities.GetUnitByViewID(heroViewID);
        hero?.IncrementHealth();
    }

    [PunRPC]
    public void ForcefieldRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit?.SetImmune(true);
    }

    [PunRPC]
    public void TeleportRPC(int unitViewID, int newX, int newY, int newZ)
    {
        UnitManager.Instance.TeleportUnit(Utilities.GetUnitByViewID(unitViewID), new Vector3Int(newX, newY, newZ));
    }

    [PunRPC]
    public void PartyTimeRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit.Reset();
    }

    [PunRPC]
    public void NavigationRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit?.IncrementMove(3);
    }

    [PunRPC]
    public void ChillingWindRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit.SetChilled(true);
    }

    [PunRPC]
    public void MushiCurseRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit.DisableSkills(true);
    }

    [PunRPC]
    public void HeroCurseRPC(int heroViewID)
    {
        BaseHero hero = (BaseHero)Utilities.GetUnitByViewID(heroViewID);
        hero.SetCursed(true);
        hero.DisableSkills(true);
    }
}
