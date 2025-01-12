using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    public InputField createInput;
    public InputField joinInput;
    public InputField nameInput;
    public GameObject createPanel;
    public GameObject roomPanel;

    [Header("Room Panel UI")]
    public TMP_Text roomName;
    public Transform playerList;
    public GameObject playerNamePrefab;

    //set player nickname before joining/creating a room
    public void SetPlayerName()
    {
        if (nameInput.text.Length >= 1)
        {
            PhotonNetwork.NickName = nameInput.text;
            Debug.Log("Player Name Set: " + PhotonNetwork.NickName);
        }
        else
        {
            Debug.LogWarning("Player name cannot be empty!");
        }
    }

    public void CreateRoom()
    {
        if (createInput.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(createInput.text, new RoomOptions() { MaxPlayers = 2 });
        }
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        createPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;

        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        //clear existing entries
        foreach (Transform child in playerList)
        {
            Destroy(child.gameObject);
        }

        //add new player entries
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerEntry = Instantiate(playerNamePrefab, playerList);
            TMP_Text playerNameText = playerEntry.GetComponentInChildren<TMP_Text>();
            playerNameText.text = player.NickName;
        }
    }
}
