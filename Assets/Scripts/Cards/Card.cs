using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CardMovement))]

public class Card : MonoBehaviour
{
    [field: SerializeField] public ScriptableCard cardData {  get; private set; }

    [SerializeField] private Image _cardImage;

    public CardEffectType cardType() => cardData.effectType;

    public void SetUp(ScriptableCard data)
    {
        cardData = data;
        _cardImage.sprite = cardData.Image; //Set image
    }

    public void PlayEffect()
    {
        cardData.PlayEffect();
    }
}
