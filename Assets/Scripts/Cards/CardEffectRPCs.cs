using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardEffectRPCs : MonoBehaviour
{
    [PunRPC]
    public void HealthOrbRPC(int heroViewID)
    {
        BaseHero hero = (BaseHero) Utilities.GetUnitByViewID(heroViewID);
        hero?.IncrementHealth();
    }
}
