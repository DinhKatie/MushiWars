using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardUI))] //requires CardUI for each Card script
[RequireComponent(typeof(CardMovement))]

public class Card : MonoBehaviour
{
    [field: SerializeField] public ScriptableCard cardData {  get; private set; }

    public void SetUp(ScriptableCard data)
    {
        cardData = data;
        GetComponent<CardUI>().SetCardUI();
    }

    public void PlayEffect()
    {
        cardData.PlayEffect();
    }
}
