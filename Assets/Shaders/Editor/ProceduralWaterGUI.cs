using System.Linq;
using UnityEditor;
using UnityEngine;

public class ProceduralWaterGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        if (!material.shaderKeywords.Contains("USE_FLOW_MAP"))
        {
            properties = properties.Where(prop => !new[] { "_FlowTex", "_FlowRate", "_FlowFade" }.Contains(prop.name)).ToArray();
        }

        base.OnGUI(materialEditor, properties);
    }
}
