using System;
using UnityEngine;

public class GeoTest : MonoBehaviour {
    public TextAsset GeoJsonAsset;
    public MapRenderer MapRenderer;
    public Material Material;
    public float Width;

    private void Start()
    {
        this.MapRenderer.SetGeoJson(this.GeoJsonAsset);
        this.MapRenderer.Render(DrawPolyLine);
    }

    private void DrawPolyLine(Vector2[] points)
    {
        GameObject child = new GameObject("Line");
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        LineRenderer renderer = child.AddComponent<LineRenderer>();
        renderer.material = this.Material;
        renderer.widthMultiplier = this.Width;
        renderer.positionCount = points.Length;
        renderer.SetPositions(Array.ConvertAll(points, v2 => (Vector3)v2));
    }
}
