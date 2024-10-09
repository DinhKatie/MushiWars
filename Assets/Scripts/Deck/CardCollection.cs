using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Collection")]
public class CardCollection : ScriptableObject
{
    [field: SerializeField] public List<ScriptableCard> CardsInCollection {  get; private set; }

    public void RemoveCardFromCollection(ScriptableCard card)
    {
        if (CardsInCollection.Contains(card))
            CardsInCollection.Remove(card);
        else
            Debug.Log("CardData is not present in Collection");
    }

    public void AddCardToCollection(ScriptableCard card)
    {
        CardsInCollection.Add(card); //Allow duplicate cards
    }
}
