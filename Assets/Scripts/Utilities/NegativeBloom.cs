using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegativeBloom : MonoBehaviour
{
    public float r, g, b;


    private void OnValidate()
    {
        Color color = new Color(r, g, b);

        GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", color);
    }
}
