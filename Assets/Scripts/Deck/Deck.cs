using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public class Deck : MonoBehaviour
{
    [SerializeField] private CardCollection _playerDeck;
    [SerializeField] private Card _cardPrefab;

    [SerializeField] private Canvas _cardCanvas;

    //Instantiate cards once into the object pool, then setActive(false) to change their status
    [field: SerializeField] public List<Card> _deckPile = new();
    public List<Card> _discardPile = new();

    [field: SerializeField] public List<Card> HandCards { get; private set; }

    private void Awake()
    {
        HandCards = new List<Card>();
        if (PhotonNetwork.IsMasterClient)
            InstantiateDeck();
    }

    private void InstantiateDeck()
    {
        for (int i = 0; i < _playerDeck.CardsInCollection.Count; i++)
        {
            GameObject cardGO = PhotonNetwork.Instantiate(_cardPrefab.name, _cardCanvas.transform.position, Quaternion.identity);
            Card card = cardGO.GetComponent<Card>();
            card.SetUp(_playerDeck.CardsInCollection[i]);
            GetComponent<PhotonView>().RPC("SetUpCardRPC", RpcTarget.Others, card.GetComponent<PhotonView>().ViewID, i);
            _deckPile.Add(card); //All cards in deck, none in hand, none in discard
            card.gameObject.SetActive(false);
        }
        GetComponent<PhotonView>().RPC("ShuffleRPC", RpcTarget.All);

    }

    [PunRPC]
    public void SetUpCardRPC(int viewID, int index)
    {
        Card card = PhotonView.Find(viewID).gameObject.GetComponent<Card>();
        card.SetUp(_playerDeck.CardsInCollection[index]);
    }

    //Call at Start and whenever Deck is empty
    //Fisher Yates from internet
    private void Shuffle()
    {
        for (int i = _deckPile.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i+1);
            var temp = _deckPile[i];
            _deckPile[i] = _deckPile[j];
            _deckPile[j] = temp;
        }
    }

    public event System.Action OnCardDrawn;
    public event System.Action OnCardDiscarded;


    public IEnumerator DrawHand(int amount = 5)
    {
        for (int i = 0; i < amount; i++)
        {
            if (_deckPile.Count <= 0)
            {
                GetComponent<PhotonView>().RPC("ShuffleRPC", RpcTarget.MasterClient);

                //Wait for reshuffle to complete
                yield return new WaitForSeconds(0.5f);
            }

            if (_deckPile.Count > 0)
            {
                HandCards.Add(_deckPile[0]);
                _deckPile[0].gameObject.SetActive(true);
                GetComponent<PhotonView>().RPC("DrawCardRPC", RpcTarget.All);
                OnCardDrawn?.Invoke();
            }

            if (_deckPile.Count <= 0)
            {
                GetComponent<PhotonView>().RPC("ShuffleRPC", RpcTarget.MasterClient);
                yield return new WaitForSeconds(0.5f);
            }

        }
        HandManager.Instance.ArrangeCardsInHand();
    }

    [PunRPC]
    public void DrawCardRPC()
    {
        //Debug.Log($"[DrawCardRPC] Removing {_deckPile[0]} from deck. Cards remaining: {_deckPile.Count}");
        _deckPile.RemoveAt(0);
        //Debug.Log($"[DrawCardRPC] Card drawn. Cards remaining: {_deckPile.Count}");
    }

    [PunRPC]
    public void ShuffleRPC()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //Debug.Log("[ShuffleRPC] Master client shuffling the deck...");
            //Debug.Log($"[ShuffleRPC] Deck size before shuffle: {_deckPile.Count}. Discard pile size: {_discardPile.Count}");

            _deckPile.AddRange(_discardPile);
            _discardPile.Clear();
            Shuffle();
            SendCardIDs();

            //Debug.Log($"[ShuffleRPC] Deck shuffled. New deck size: {_deckPile.Count}. Discard pile is now empty.");

            Debug.Log("[ShuffleRPC] New deck order:");
            for (int i = 0; i < _deckPile.Count; i++)
            {
                var card = _deckPile[i];
                Debug.Log($"Card {i + 1}: ID = {card.GetComponent<PhotonView>().ViewID}, Name = {card.name}");
            }
        }
    }

    private void SendCardIDs()
    {
        List<int> cardIds = new List<int>();
        foreach (var card in _deckPile)
        {
            cardIds.Add(card.GetComponent<PhotonView>().ViewID);
        }

        GetComponent<PhotonView>().RPC("UpdateDeckState", RpcTarget.All, cardIds.ToArray());
    }

    [PunRPC]
    public void UpdateDeckState(int[] cardIds)
    {
        Debug.Log("[UpdateDeckState] Updating deck state on all clients...");
        Debug.Log($"[UpdateDeckState] Received card IDs. Count: {cardIds.Length}");
        _deckPile.Clear();
        foreach (int id in cardIds)
        {
            var card = PhotonView.Find(id)?.gameObject.GetComponent<Card>();
            if (card != null)
                _deckPile.Add(card);
            else
                Debug.LogWarning($"[UpdateDeckState] Card with PhotonView ID {id} not found.");
        }

        Debug.Log($"[UpdateDeckState] Deck updated. New deck size: {_deckPile.Count}");
    }

    [PunRPC]
    public void DiscardCardRPC(int viewID)
    {
        Debug.Log($"[DiscardCardRPC] Discarding card with PhotonView ID {viewID}");

        Card card = PhotonView.Find(viewID)?.gameObject.GetComponent<Card>();
        if (card != null)
        {
            _discardPile.Add(card);
            Debug.Log($"[DiscardCardRPC] Card discarded. Discard pile size: {_discardPile.Count}");
        }
        else
            Debug.LogWarning("[DiscardCardRPC] Card is null!");
    }

    [PunRPC]
    public void DisplayCardToPlayersRPC(int viewID)
    {
        PhotonView cardView = PhotonView.Find(viewID);
        if (cardView != null)
        {
            Card card = cardView.GetComponent<Card>();
            ShowPlayedCard(card);
        }
    }

    void ShowPlayedCard(Card card)
    {
        GetComponent<PlayedCardDisplay>().DisplayCard(card);
    }

    //No cards can be discarded from deck to discard
    //Only from hand to discard
    public void DiscardCard(Card card)
    {
        if (HandCards.Contains(card))
        {
            HandCards.Remove(card);
            GetComponent<PhotonView>().RPC("DiscardCardRPC", RpcTarget.MasterClient, card.GetComponent<PhotonView>().ViewID);
            GetComponent<PhotonView>().RPC("DisplayCardToPlayersRPC", RpcTarget.All, card.GetComponent<PhotonView>().ViewID);

            card.gameObject.SetActive(false);
            HandManager.Instance.UpdateCardCount();

            if (HandManager.Instance.discardingForCardEffect)
            {
                OnCardDiscarded?.Invoke();
                StartCoroutine(ResetDiscardFlag());
            }

            card.PlayEffect(); //Apply its effect
        }
    }

    private IEnumerator ResetDiscardFlag()
    {
        yield return null; //Wait a frame to allow event to process
        HandManager.Instance.discardingForCardEffect = false;
    }
}
