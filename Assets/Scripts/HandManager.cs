using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HandManager : MonoBehaviour
{
    public Deck deck;
    [SerializeField] private Transform cardParent;
    [SerializeField] private Vector2 cardOffset = new Vector2(100f, 0f); // The distance between each card

    private List<Card> handCards => deck.HandCards;
    private int MAX_HAND_SIZE = 3;

    public static HandManager Instance;

    public bool discardingForCardEffect = false;

    public int numCards => handCards.Count;
    public bool DisableCardEffects() => discardingForCardEffect;
    public bool hasMaxHandSize() => handCards.Count >= MAX_HAND_SIZE;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ArrangeCardsInHand();
    }

    public void DrawACard()
    {
        if (numCards < MAX_HAND_SIZE)
        {
            StartCoroutine(deck.DrawHand(1));
            UpdateCardCount();
        }
    }

    public void UpdateCardCount()
    {
        GetComponent<OpponentHand>().UpdateHandCount(PhotonNetwork.LocalPlayer.ActorNumber, numCards);
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
            csh.ResetCard();

        }
    }

}
