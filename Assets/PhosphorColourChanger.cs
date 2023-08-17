using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhosphorColourChanger : MonoBehaviour
{
    private PhosphorScreenRenderer phosphorGraphics;

    private Color currentColour = Color.white;
    private Color retrievedColour = Color.black;

    private void Start()
    {
        phosphorGraphics = GetComponent<PhosphorScreenRenderer>();

        float hue = PlayerPrefs.GetFloat("InterfaceHue");
        Color retrievedColour = Color.HSVToRGB(hue, 0.9f, 1f, true);
        retrievedColour.a = 0.6f;

        currentColour = retrievedColour;
        phosphorGraphics.PhosphorColor = currentColour;
    }

    // Update is called once per frame
    void Update ()
    {
        float hue = PlayerPrefs.GetFloat("InterfaceHue");
        Color retrievedColour = Color.HSVToRGB(hue, 0.9f, 1f, true);
        retrievedColour.a = 0.6f;

        if (currentColour != retrievedColour)
        {
            phosphorGraphics.PhosphorColor = retrievedColour;
            currentColour = retrievedColour;
        }
    }
}
