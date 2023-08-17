using System.Collections.Generic;
using TotalDistraction.TextureAtlasCreator;
using UnityEngine;

public class RuntimeUVRemap : MonoBehaviour
{
    public List<Rect> TextureRects;
    public List<Material> Materials;
    public UVRemapMode Mode;

    private void Awake()
    {
        if (this.Mode == UVRemapMode.CalculateOnAwake)
        {
            CalculateMeshUVs();
        }
        else if (this.Mode == UVRemapMode.UseInstancedShaders)
        {
            CalculatePerInstanceShaderData();
        }
    }

    private void CalculateMeshUVs()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if (filter == null || renderer == null) return;

        filter.mesh = UVRemap.Forward(filter.sharedMesh, this.TextureRects.ToArray());
        renderer.sharedMaterials = this.Materials.ToArray();
    }

    private void CalculatePerInstanceShaderData()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if (filter == null || renderer == null) return;

        Rect r = this.TextureRects[0];

        Vector4 tilingOffset = new Vector4(r.width, r.height, r.x, r.y);

        MaterialPropertyBlock block = new MaterialPropertyBlock();

        renderer.sharedMaterials = this.Materials.ToArray();

        renderer.GetPropertyBlock(block);

        block.SetVector("_MainTex_ST", tilingOffset);

        renderer.SetPropertyBlock(block);
    }
}
