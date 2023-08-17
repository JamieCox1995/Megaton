using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveObject : MonoBehaviour
{
    public float timeToDissolve;
    public static Shader dissolveShader;

    private static Dictionary<Material, Material> materialDictionary = new Dictionary<Material, Material>();

    private MeshRenderer m_renderer;
    private Dictionary<string, int> _shaderPropertyIDs;

    public void StartDissolve(float dissolveTime)
    {
        m_renderer = GetComponent<MeshRenderer>();

        if (!m_renderer) return;

        m_renderer.sharedMaterials = SwapMaterialShader(dissolveShader, m_renderer.sharedMaterials);

        _shaderPropertyIDs = new Dictionary<string, int> {  { "_DissolvePercentage", Shader.PropertyToID("_DissolvePercentage") }};

        timeToDissolve = dissolveTime;

        StartCoroutine("Dissolve");
    }

    public void StartDissolve(Color dissolveColour, float dissolveTime, float prop, float scale)
    {
        m_renderer = GetComponent<MeshRenderer>();

        if (!m_renderer) return;

        m_renderer.sharedMaterials = SwapMaterialShader(dissolveShader, m_renderer.sharedMaterials);

        _shaderPropertyIDs = new Dictionary<string, int> {  { "_EmissionColor", Shader.PropertyToID("_EmissionColor")},
                                                            { "_DissolvePropagation", Shader.PropertyToID("_DissolvePropagation") },
                                                            {"_DissolveScale", Shader.PropertyToID("_DissolveScale") },
                                                            {"_DissolvePercentage", Shader.PropertyToID("_DissolvePercentage") } };

        timeToDissolve = dissolveTime;

        MaterialPropertyBlock propBlock = MaterialManager.Instance[m_renderer];

        propBlock.SetColor(_shaderPropertyIDs["_EmissionColor"], dissolveColour);
        propBlock.SetFloat(_shaderPropertyIDs["_DissolvePropagation"], prop);
        propBlock.SetFloat(_shaderPropertyIDs["_DissolveScale"], scale);

        MaterialManager.Instance[m_renderer] = propBlock;

        StartCoroutine("Dissolve");
    }

    private IEnumerator Dissolve()
    {
        float timer = 0f;

        while (timer <= timeToDissolve)
        {
            timer += Time.deltaTime;

            float amount = timer / timeToDissolve;

            MaterialPropertyBlock propBlock = MaterialManager.Instance[m_renderer];

            propBlock.SetFloat("_DissolvePercentage", amount);

            MaterialManager.Instance[m_renderer] = propBlock;

            yield return null;
        }

        Destroy(gameObject);
    }

    //public void SetMaterialShader(string shaderType)
    //{
    //    m_renderer = GetComponent<MeshRenderer>();

    //    Shader newShader = Shader.Find(shaderType);

    //    Material material = m_renderer.sharedMaterial;
    //    SetMaterialShader(newShader, material);
    //    //else
    //    //{
    //    //    Debug.LogFormat("Shader not found: {0}", shaderType);
    //    //}
    //}

    private Material[] SwapMaterialShader(Shader newShader, Material[] materials)
    {
        Material[] result = new Material[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            result[i] = SwapMaterialShader(newShader, materials[i]);
        }

        return result;
    }

    private Material SwapMaterialShader(Shader newShader, Material material)
    {
        if (newShader != null)
        {
            if (material.shader == newShader) return material;

            if (materialDictionary.ContainsKey(material))
            {
                return materialDictionary[material];
            }
            else
            {
                Material newMat = Material.Instantiate(material);

                newMat.shader = newShader;
                newMat.name = string.Format("{0} ({1})", material.name, newShader.name);

                //m_renderer.sharedMaterial = newMat;

                materialDictionary.Add(material, newMat);

                return newMat;
            }
        }

        return null;
    }
}
