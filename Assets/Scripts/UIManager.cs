using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void InfoScene()
    {
        SceneManager.LoadScene(2);
    }

    public void Url()
    {
        Application.OpenURL("https://www.instagram.com/mariya_songshine/");
    }

}
