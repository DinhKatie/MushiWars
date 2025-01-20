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
}
