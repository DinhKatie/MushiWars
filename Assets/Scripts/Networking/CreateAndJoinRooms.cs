using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public TMP_InputField nameInput;
    public GameObject createPanel;
    public GameObject roomPanel;

    [Header("Room Panel UI")]
    public TMP_Text roomName;
    public Transform playerList;
    public GameObject playerNamePrefab;
    public GameObject warningText;

    private Coroutine warningCoroutine;

    public void SetWarningText(string msg)
    {
        //stop the currently running warning coroutine if any
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }
        warningCoroutine = StartCoroutine(DisplayWarningText(msg));
    }

    public IEnumerator DisplayWarningText(string msg)
    {
        warningText.GetComponent<TextMeshProUGUI>().text = msg;
        warningText.SetActive(true);

        yield return new WaitForSeconds(4f);

        warningText.SetActive(false);

        yield return null;
    }
    //set player nickname before joining/creating a room
    public void SetPlayerName()
    {
        if (nameInput.text.Length >= 1)
        {
            PhotonNetwork.NickName = nameInput.text;
            SetWarningText("Player Name Set: " + PhotonNetwork.NickName);
        }
        else
        {
            SetWarningText("Player name cannot be empty!");
        }
    }

    public void CreateRoom()
    {
        if (string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            SetWarningText("Player name cannot be empty!");
            return;
        }

        if (string.IsNullOrWhiteSpace(createInput.text))
        {
            SetWarningText("Room name cannot be empty!");
            return;
        }
        
        PhotonNetwork.CreateRoom(createInput.text, new RoomOptions() { MaxPlayers = 2 });
    }

    public void JoinRoom()
    {
        if (string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            SetWarningText("Player name cannot be empty!");
            return;
        }

        if (string.IsNullOrWhiteSpace(joinInput.text))
        {
            SetWarningText("Room name cannot be empty!");
            return;
        }

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

    public void StartGame() //Start Game Button
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.LoadLevel("SampleScene");
                SceneManager.sceneLoaded += OnSceneLoaded; //wait until the scene is fully loaded, then run OnSceneLoaded
            }
            else
                SetWarningText("Not enough players to start the game.");
        }
        else
            SetWarningText("Only the host can start the game.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            //ensure TurnManager is initialized and then start the game
            if (TurnManager.Instance != null)
                TurnManager.Instance.StartGame();
            else
                Debug.LogError("TurnManager is null!");
        }
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        createPanel.SetActive(true);
    }
}
