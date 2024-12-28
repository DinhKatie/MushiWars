using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "CardData")] //Create new CardData Object with right-click in the editor
public class ScriptableCard : ScriptableObject
{
    [field: SerializeField] public string CardName { get; private set; }
    [field: SerializeField, TextArea] public string CardDescription { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [SerializeField] public CardEffectType effectType;

    // Define a delegate for card effects
    public System.Action OnPlayEffect;

    public void PlayEffect()
    {
        OnPlayEffect?.Invoke();
    }
}

public enum CardEffectType
{
    Skill,
    Hex,
    Support
}
