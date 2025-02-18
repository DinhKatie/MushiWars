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

        Tooltips.Instance.Show("+1 Move", Color.white, unit.CurrentPosition, Vector2.up * 100);
        Utilities.PlaySound("buff1");
    }

    [PunRPC]
    public void BlastStompRPC()
    {
        Utilities.PlaySound("blaststomp");
    }

    [PunRPC]
    public void SmiteRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit.PlayEffectAnimation("Smite");
        Utilities.PlaySound("smite");
    }

    [PunRPC]
    public void HealthOrbRPC(int heroViewID)
    {
        BaseHero hero = (BaseHero) Utilities.GetUnitByViewID(heroViewID);
        hero?.IncrementHealth();

        Utilities.PlaySound("healthorb");
        Tooltips.Instance.Show("+1 Health!", Color.white, hero.CurrentPosition, Vector3.up * 100);
    }

    [PunRPC]
    public void ForcefieldRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit?.SetImmune(true);

        Utilities.PlaySound("forcefield");
    }

    [PunRPC]
    public void TeleportRPC(int unitViewID, int newX, int newY, int newZ)
    {
        UnitManager.Instance.TeleportUnit(Utilities.GetUnitByViewID(unitViewID), new Vector3Int(newX, newY, newZ));

        Utilities.PlaySound("teleportation");
    }

    [PunRPC]
    public void PartyTimeRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit.Reset();

        Utilities.PlaySound("partytime");
        Tooltips.Instance.Show("Turn Refreshed!", Color.blue, unit.CurrentPosition, Vector2.up * 100);
    }

    [PunRPC]
    public void Navigation1RPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);

        Utilities.PlaySound("navigation1");
        Tooltips.Instance.Show("Turn Cancelled.", Color.blue, unit.CurrentPosition, Vector3.up * 100);
    }

    [PunRPC]
    public void NavigationRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit?.IncrementMove(3);

        Utilities.PlaySound("navigation2");
        Tooltips.Instance.Show("+3 Move", Color.white, unit.CurrentPosition, Vector3.up * 100);
    }

    [PunRPC]
    public void ChillingWindRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit.SetChilled(true);

        Utilities.PlaySound("chillingwind");
    }

    [PunRPC]
    public void MushiCurseRPC(int unitViewID)
    {
        BaseUnit unit = Utilities.GetUnitByViewID(unitViewID);
        unit.SetCursed(true);
    }

    [PunRPC]
    public void HeroCurseRPC(int heroViewID)
    {
        BaseHero hero = (BaseHero)Utilities.GetUnitByViewID(heroViewID);
        hero.SetCursed(true);
        hero.DisableSkills(true);
    }
}
