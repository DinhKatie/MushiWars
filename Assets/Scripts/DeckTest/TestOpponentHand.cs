using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOpponentHand : MonoBehaviour
{
    public GameObject faceDownCardPrefab;
    public Transform opponentHandArea;
    private Vector2 cardOffset = new Vector2(250f, 1.5f); // The distance between each card, and how much is visible in the frame (y)

    private Dictionary<int, int> playerHandCounts = new Dictionary<int, int>();

    public void UpdateHandCount(int playerID, int handCount)
    {
        GetComponent<PhotonView>().RPC("UpdateHandCountRPC", RpcTarget.All, playerID, handCount);
        StartCoroutine(DelayedUpdateUI(playerID));
    }

    private IEnumerator DelayedUpdateUI(int playerID)
    {
        //wait to ensure cards are synced first
        yield return null;

        GetComponent<PhotonView>().RPC("UpdateUI", RpcTarget.Others, playerID);
    }

    [PunRPC]
    public void UpdateHandCountRPC(int playerID, int handCount)
    {
        playerHandCounts[playerID] = handCount;

        foreach (var player in playerHandCounts.Keys)
            Debug.Log($"Updated Hand Counts: Player {GetPlayer(player)} has {playerHandCounts[player]} cards.");
    }

    [PunRPC]
    public void UpdateUI(int playerID)
    {
        Debug.Log($"Updating UI for player: {GetPlayer(playerID)}");
        UpdateHandUI(playerID); //Update this player's hand in everyone else's view
    }

    public string GetPlayer(int playerID)
    {
        // Check if player with the given ID exists
        if (PhotonNetwork.PlayerList != null)
        {
            // Iterate through the player list to find the player by their actor number
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == playerID)
                {
                    return player.NickName;  // Get the player's name (NickName)
                }
            }
        }

        // If no player was found with that ID
        Debug.LogWarning("Player with ID " + playerID + " not found.");
        return "None";
    }

    private void UpdateHandUI(int playerID)
    {
        //clear existing UI
        foreach (Transform child in opponentHandArea)
            Destroy(child.gameObject);

        // Display face-down cards based on hand count
        if (playerHandCounts.TryGetValue(playerID, out int handCount))
        {
            Debug.Log($"Player: {GetPlayer(playerID)} has {handCount} cards");
            List<GameObject> cards = new List<GameObject>();
            for (int i = 0; i < handCount; i++)
            {
                Debug.Log("Instantiating Card.");
                GameObject cardInstance = Instantiate(faceDownCardPrefab, opponentHandArea);
                cards.Add(cardInstance);
                ArrangeCardsInOpponentHand(cards);
            }
        }
    }

    public void ArrangeCardsInOpponentHand(List<GameObject> cards)
    {
        Debug.Log("Arranging Opponent Cards");
        if (cards.Count == 0) return;

        float handWidth = (cards.Count - 1) * cardOffset.x;

        // center cards if less than max hand count
        float startX = -handWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform cardTransform = cards[i].GetComponent<RectTransform>();

            // Position the card with dynamic spacing and centered
            float newXPosition = startX + (i * cardOffset.x);
            float newYPosition = cardTransform.rect.height / cardOffset.y;
            cardTransform.anchoredPosition = new Vector2(newXPosition, newYPosition);
        }
    }
}
