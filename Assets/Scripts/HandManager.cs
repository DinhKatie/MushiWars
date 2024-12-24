using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [SerializeField] private Transform cardParent;
    [SerializeField] private Vector2 cardOffset = new Vector2(100f, 0f); // The distance between each card

    private List<Card> handCards => Deck.Instance.HandCards;

    public static HandManager Instance;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        ArrangeCardsInHand();
    }

    // Call this whenever the hand changes (e.g., after drawing, discarding, or end of a turn)
    public void ArrangeCardsInHand()
    {
        if (handCards.Count == 0) return;

        // Determine the total width for the cards, based on how many are in the hand
        float handWidth = (handCards.Count - 1) * cardOffset.x;

        // Center the cards if less than the max hand count
        float startX = -handWidth / 2f;

        for (int i = 0; i < handCards.Count; i++)
        {
            RectTransform cardTransform = handCards[i].GetComponent<RectTransform>();
            cardTransform.SetParent(cardParent);

            // Position the card with dynamic spacing and centered
            float newXPosition = startX + (i * cardOffset.x);
            cardTransform.anchoredPosition = new Vector2(newXPosition, 0f);

            CardSelectionHandler csh = handCards[i].GetComponent<CardSelectionHandler>();
            csh.Reset();

        }
    }

}
