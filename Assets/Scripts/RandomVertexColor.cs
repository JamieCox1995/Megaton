using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RandomVertexColor : MonoBehaviour
{
    [ColorUsage(false, true, 0f, 8f, 0.125f, 3f)]
    public Color BaseColor;
    [Range(0f, 1f)]
    public float HueVariance;
    [Range(0f, 1f)]
    public float SaturationVariance;
    [Range(0f, 1f)]
    public float LightnessVariance;

    private void OnValidate()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        AssignVertexColors(meshFilter.sharedMesh);
    }

    //private void Start()
    //{
    //    if (Application.isEditor)
    //    {
    //        OnValidate();
    //    }
    //    else
    //    {
    //        MeshFilter meshFilter = GetComponent<MeshFilter>();

    //        if (meshFilter == null || meshFilter.mesh == null) return;

    //        AssignVertexColors(meshFilter.mesh);
    //    }
    //}

    private void AssignVertexColors(Mesh mesh)
    {
        if (mesh == null) throw new System.ArgumentNullException("mesh");

        int vertexCount = mesh.vertexCount;

        Color[] vertexColors = new Color[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            vertexColors[i] = RandomHSL(this.BaseColor, this.HueVariance, this.SaturationVariance, this.LightnessVariance);

            //Debug.LogFormat("{0}[{1}]: {2}", mesh.name, i, vertexColors[i]);

            //for (int j = 0; j < 3; j++)
            //{
            //    if (vertexColors[i][j] < 0) Debug.LogFormat("Vertex {0} on mesh {1} has negative value for channel {2}", i, mesh.name, j);
            //}
        }

        mesh.colors = vertexColors;
    }

    private static Color RandomHSL(Color baseColor, float hueVariance, float saturationVariance, float lightnessVariance)
    {
        float baseHue, baseSaturation, baseLightness;

        Color.RGBToHSV(baseColor, out baseHue, out baseSaturation, out baseLightness);

        float hue = baseHue + Random.Range(-hueVariance, hueVariance);

        if (hue < 0f) hue = 1f + (hue % 1);
        if (hue > 1f) hue = hue % 1;

        float saturation = Mathf.Clamp01(baseSaturation + Random.Range(-saturationVariance, saturationVariance));

        float lightness = Mathf.Max(0, baseLightness + Random.Range(-lightnessVariance, lightnessVariance));

        return Color.HSVToRGB(hue, saturation, lightness, true);
    }
}
