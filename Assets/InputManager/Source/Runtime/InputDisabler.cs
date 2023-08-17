using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TeamUtility.IO;


public class InputDisabler : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //InputManager.DisableAllAxes("Keyboard Default");
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(InputManager.GetKeyDown(KeyCode.Space))
        {
            //InputManager.DisableAllAxes("Keyboard Default");
        }
	}
}
