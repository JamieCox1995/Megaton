using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickUnlock : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
	    if (!PlayerPrefs.HasKey("Primary"))
        {
            PlayerPrefs.SetString("Primary", "Standard");
        }	

        if (!PlayerPrefs.HasKey("Secondary"))
        {
            PlayerPrefs.SetString("Primary", "Homing");
        }
	}
	
    public void SetPrimary(string ammo)
    {
        PlayerPrefs.SetString("Primary", ammo);
    }

    public void SetSecondary(string ammo)
    {
        PlayerPrefs.SetString("Secondary", ammo);
    }
}
