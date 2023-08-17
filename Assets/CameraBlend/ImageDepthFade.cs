using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ImageDepthFade : MonoBehaviour
{
    public RenderTexture EffectTexture;
    public float Depth;
    public float CrossfadeDistance;
    public float SkyFadeStartDistance;
    public float SkyFadeEndDistance;
    public Shader DepthFadeShader;

    private Material _material;
    private Camera _mainCamera;
    private Camera _effectCamera;

    private bool _useTemporaryRenderTexture;

    private void Reset()
    {
        _effectCamera = GetEffectCamera();
    }

    private Camera GetEffectCamera()
    {
        Camera result = null;

        foreach (Transform child in transform)
        {
            result = child.GetComponent<Camera>();

            if (_effectCamera != null) break;
        }

        if (result == null)
        {
            GameObject gameObject = new GameObject("Effect Camera");
            gameObject.transform.SetParent(this.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            result = gameObject.AddComponent<Camera>();
        }

        return result;
    }

    // Use this for initialization
    private void Start()
    {
        _useTemporaryRenderTexture = this.EffectTexture == null;
        _material = new Material(this.DepthFadeShader);
        _mainCamera = GetComponent<Camera>();

        if (_mainCamera == null) this.enabled = false;

        if (!_useTemporaryRenderTexture) return;

        _effectCamera = GetEffectCamera();

        if (_effectCamera == null)
        {
            this.enabled = false;
        }
        else
        {
            _effectCamera.enabled = false;
        }
	}

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_useTemporaryRenderTexture)
        {
            this.EffectTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            _effectCamera.targetTexture = this.EffectTexture;
            _effectCamera.Render();
        }

        _material.SetTexture("_EffectTex", this.EffectTexture);
        _material.SetFloat("_Depth", ToCameraDepth(this.Depth));
        _material.SetFloat("_CrossfadeAmount", ToCameraDepth(this.CrossfadeDistance));
        _material.SetFloat("_SkyDepthFrom", ToCameraDepth(this.SkyFadeStartDistance));
        _material.SetFloat("_SkyDepthTo", ToCameraDepth(this.SkyFadeEndDistance));

        Graphics.Blit(source, destination, _material);

        if (_useTemporaryRenderTexture)
        {
            _effectCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(this.EffectTexture);
            this.EffectTexture = null;
        }
    }

    private float ToCameraDepth(float distance)
    {
        return Mathf.InverseLerp(_mainCamera.nearClipPlane, _mainCamera.farClipPlane, distance);
    }
}
