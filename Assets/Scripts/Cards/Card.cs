using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardUI))] //requires CardUI for each Card script
public class Card : MonoBehaviour
{
    [field: SerializeField] public ScriptableCard cardData {  get; private set; }
}
