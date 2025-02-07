using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestDeck : MonoBehaviour
{
    [SerializeField] private CardCollection _playerDeck;
    [SerializeField] private Card _cardPrefab;
    [SerializeField] public TMP_Text deckText;

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
        UpdateDeckText();
        //GetComponent<PhotonView>().RPC("ShuffleRPC", RpcTarget.All);

    }

    [PunRPC]
    public void SetUpCardRPC(int viewID, int index)
    {
        Card card = PhotonView.Find(viewID).gameObject.GetComponent<Card>();
        card.SetUp(_playerDeck.CardsInCollection[index]);
    }

    public void UpdateDeckText()
    {
        deckText.text = "";
        for (int i = 0; i < _deckPile.Count; i++)
        {
            var card = _deckPile[i];
            deckText.text += $"Card {i + 1}: ID = {card.GetComponent<PhotonView>().ViewID}, Name = {card.name}\n";
        }

    }
}
