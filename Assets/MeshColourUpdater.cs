using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColourUpdater : MonoBehaviour
{
    public GameObject meshObject;
    private Color currentMeshColour;
	
	// Update is called once per frame
	void Update ()
    {
        float hue = PlayerPrefs.GetFloat("InterfaceHue");
        Color retrievedColour = Color.HSVToRGB(hue, 1f, 2f, true);

        if (currentMeshColour != retrievedColour)
        {
            meshObject.GetComponent<Renderer>().material.SetColor("_WireColor", retrievedColour);
            currentMeshColour = retrievedColour;
        }
	}
}
