using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpEmissiveColour : MonoBehaviour
{

    public Color _targetColor = Color.black;
    public float lerpTime = 10f;
    public int materialIndex = 1;

    private Renderer rend;
    [ColorUsage(false, true, 0f, 8f, 0.125f, 3f)]
    private Color _startColor;

    private int _shaderPropertyID;

    private void Start()
    {
        _shaderPropertyID = Shader.PropertyToID("_EmissionColor");

        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }

        if (rend == null)
        {
            this.enabled = false;
        }

        if (materialIndex >= 0 && materialIndex < rend.sharedMaterials.Length)
        {
            Material material = rend.sharedMaterials[materialIndex];

            if (material == null)
            {
                this.enabled = false;
            }
            else
            {
                _startColor = rend.sharedMaterials[materialIndex].GetColor(_shaderPropertyID);
            }
        }
        else
        {
            this.enabled = false;
        }
    }

    public void ResetEmission()
    {
        if (this.enabled)
        {
            if (rend)
            {
                SetEmissionColor(_startColor);
            }
        }
    }

    public void Interpolate()
    {
        if (this.enabled && gameObject.activeInHierarchy)
        {
            StartCoroutine(InterpolateInternal());
        }
    }

    private IEnumerator InterpolateInternal()
    {
        if (rend == null) yield break;

        float timer = 0f;

        while (timer <= lerpTime)
        {
            timer += Time.deltaTime;

            float t = timer / lerpTime;

            Color color = Color.Lerp(_startColor, _targetColor, t);

            //SetEmissionColor(color);

            yield return null;
        }
    }

    private void SetEmissionColor(Color color)
    {
        MaterialPropertyBlock propertyBlock = MaterialManager.Instance[rend];

        propertyBlock.SetColor(_shaderPropertyID, color);

        MaterialManager.Instance[rend] = propertyBlock;
    }
}
