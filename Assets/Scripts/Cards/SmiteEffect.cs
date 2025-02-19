using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmiteEffect : MonoBehaviour
{
    private BaseUnit parentUnit;

    private void Awake()
    {
        parentUnit = GetComponentInParent<BaseUnit>();
    }

    public void OnSmiteEffectEnd()
    {
        if (parentUnit == null || !PhotonNetwork.IsMasterClient) return;

        //Master Client tells everyone to disable the unit
        parentUnit.GetComponent<PhotonView>().RPC("SmiteEndRPC", RpcTarget.All, parentUnit.GetComponent<PhotonView>().ViewID);
    }

}
