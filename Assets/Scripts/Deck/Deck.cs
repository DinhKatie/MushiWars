using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public static Deck Instance {  get; private set; } //Singleton

    [SerializeField] private CardCollection _playerDeck;
    [SerializeField] private Card _cardPrefab;

    [SerializeField] private Canvas _cardCanvas;

    //Instantiate cards once into the object pool, then setActive(false) to change their status
    [field: SerializeField] public List<Card> _deckPile = new();
    public List<Card> _discardPile = new();

    [field: SerializeField] public List<Card> HandCards { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        HandCards = new List<Card>();
    }

    private void Start()
    {
        InstantiateDeck();
    }

    private void InstantiateDeck()
    {
        for (int i = 0; i < _playerDeck.CardsInCollection.Count; i++)
        {
            Card card = Instantiate(_cardPrefab, _cardCanvas.transform);
            card.SetUp(_playerDeck.CardsInCollection[i]);
            _deckPile.Add(card); //All cards in deck, none in hand, none in discard
            card.gameObject.SetActive(false);
        }

        Shuffle();
    }

    //Call at Start and whenever Deck is empty
    //Fisher Yates from internet lol
    private void Shuffle()
    {
        for (int i = _deckPile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i+1);
            var temp = _deckPile[i];
            _deckPile[i] = _deckPile[j];
            _deckPile[j] = temp;
        }
    }

    public void DrawHand(int amount = 5)
    {
        for (int i = 0; i < amount; i++)
        {
            if (_deckPile.Count <= 0)
            {
                _deckPile.AddRange(_discardPile);
                _discardPile.Clear();
                Shuffle();
            }

            if (_deckPile.Count > 0)
            {
                HandCards.Add(_deckPile[0]);
                _deckPile[0].gameObject.SetActive(true);
                _deckPile.RemoveAt(0);
            }
            
        }
    }

    //No cards can be discarded from deck to discard
    //Only from hand to discard
    public void DiscardCard(Card card)
    {
        if (HandCards.Contains(card))
        {
            HandCards.Remove(card);
            _discardPile.Add(card);
            card.gameObject.SetActive(false);
        }
    }
}
