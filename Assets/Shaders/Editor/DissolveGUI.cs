using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DissolveGUI : ShaderGUI
{
    private static string[] CustomDrawnProperties =
    {
        "_EmissionColor",
        "_DissolvePropagation",
        "_DissolvePercentage",
        "_DissolveScale",
    };

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        base.OnGUI(materialEditor, properties);

        var customDrawnProps = properties.Where(prop => CustomDrawnProperties.Contains(prop.name));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Per-Renderer Properties", EditorStyles.boldLabel);

        foreach (MaterialProperty prop in customDrawnProps)
        {
            switch (prop.name)
            {
                case "_EmissionColor":
                    materialEditor.ColorProperty(prop, prop.displayName);
                    break;
                case "_DissolvePropagation":
                case "_DissolvePercentage":
                    materialEditor.RangeProperty(prop, prop.displayName);
                    break;
                case "_DissolveScale":
                    materialEditor.FloatProperty(prop, prop.displayName);
                    break;
            }
        }
    }
}
