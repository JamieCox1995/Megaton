using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrtEffect : MonoBehaviour
{
    public Shader CrtShader;
    private Material _material;

    [Header("Signal Noise"), Range(0, 1)]
    public float YNoise;
    [Range(0, 1)]
    public float PrNoise;
    [Range(0, 1)]
    public float PbNoise;

    [Header("Scanline Noise"), Range(0, 1)]
    public float HorizontalNoise;
    [Range(0, 1)]
    public float VerticalNoise;

    private void Awake()
    {
        _material = new Material(this.CrtShader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetVector("_SignalNoise", new Vector4(this.YNoise, this.PrNoise, this.PbNoise));
        _material.SetVector("_ScanNoise", new Vector4(this.HorizontalNoise / 100f, this.VerticalNoise / 100f));

        Graphics.Blit(source, destination, _material);
    }
}
