using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public EventSystem eventSystem;

    public void LoadLevel(int levelIndex)
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; 

        SceneManager.LoadScene(levelIndex);
    }

    public void SetSelectedObject(GameObject current)
    {
        eventSystem.SetSelectedGameObject(current);
        eventSystem.firstSelectedGameObject = current;

        Animator anim = current.GetComponent<Animator>();

        if (anim)
        {
            anim.SetTrigger("Highlighted");
        }
    } 

    public void Unpause()
    {
        GameManager.instance.UnpauseGame();
    }

    public void OpenMenu(GameObject toOpen)
    {
        toOpen.SetActive(true);
    }

    public void CloseMenu(GameObject toClose)
    {
        toClose.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

