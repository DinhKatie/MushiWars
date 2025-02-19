using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string loadingSceneName = "Loading";

    public void PlayGame()
    {
        SceneManager.LoadScene(loadingSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
