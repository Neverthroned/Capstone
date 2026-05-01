using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject AboutPanel;

    public void Start()
    {
        AboutPanel.SetActive(false);
    }

    public void PlayGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SceneManager.LoadScene("Invection");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void About()
    {
        AboutPanel.SetActive(!AboutPanel.activeSelf);
    }
}
