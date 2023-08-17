using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;

public class MainMenuBehaviour : MonoBehaviour
{
    public Animator mainMenuStateMachine;

    private bool playerReady = false;

	// Use this for initialization
	void Start ()
    {
        playerReady = false;	
	}
	
	// Update is called once per frame
	void Update ()
    {
		// Here we are just going to check for any input from the player.
        if (playerReady)
        {
            return;
        }

        if (InputManager.anyKey)
        {
            playerReady = true;
            SetStateBehaviour("Initial Input");
        }
	}

    public void SetStateBehaviour(string trigger)
    {
        if (mainMenuStateMachine == null)
        {
            throw new Exception("No Animator has been assigned!");
        }

        mainMenuStateMachine.SetTrigger(trigger);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
