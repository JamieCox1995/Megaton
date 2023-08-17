using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PerRendererDataShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        MaterialProperty[] baseProperties = properties.Where(prop => (prop.flags & MaterialProperty.PropFlags.PerRendererData) == 0).ToArray();

        base.OnGUI(materialEditor, baseProperties);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Per-Renderer Data", EditorStyles.boldLabel);

        MaterialProperty[] perRendererProperties = properties.Except(baseProperties).ToArray();

        foreach (MaterialProperty prop in perRendererProperties)
        {
            switch (prop.type)
            {
                case MaterialProperty.PropType.Color:
                    prop.colorValue = materialEditor.ColorProperty(prop, prop.displayName);
                    break;

                case MaterialProperty.PropType.Float:
                    prop.floatValue = materialEditor.FloatProperty(prop, prop.displayName);
                    break;

                case MaterialProperty.PropType.Range:
                    prop.floatValue = materialEditor.RangeProperty(prop, prop.displayName);
                    break;

                case MaterialProperty.PropType.Texture:
                    // Unity doesn't like [PerRendererData] textures being edited for some reason,
                    // so let's trick it into thinking it's a normal texture property with some reflection.
                    // By XOR-ing the property flags with the PerRendererData flag, we can keep the other
                    // flags set, draw the editor field, and then XOR it again to reset it to its original
                    // value.
                    System.Reflection.FieldInfo fieldInfo = typeof(MaterialProperty).GetField("m_Flags", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    fieldInfo.SetValue(prop, prop.flags ^ MaterialProperty.PropFlags.PerRendererData);


                    GUIContent label = new GUIContent(prop.displayName);
                    materialEditor.TexturePropertySingleLine(label, prop);

                    // Get and draw the tiling/offset property
                    MaterialProperty tilingProp;
                    try
                    {
                        tilingProp = FindProperty(string.Format("{0}_ST", prop.name), properties);
                    }
                    catch (System.ArgumentException)
                    {
                        tilingProp = null;
                    }

                    if (tilingProp != null)
                    {
                        Vector2 tiling, offset;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            tiling = new Vector2(tilingProp.vectorValue.x, tilingProp.vectorValue.y);
                            EditorGUILayout.PrefixLabel("Tiling");
                            tiling = EditorGUILayout.Vector2Field(string.Empty, tiling, GUILayout.MaxWidth(128));
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            offset = new Vector2(tilingProp.vectorValue.z, tilingProp.vectorValue.w);
                            EditorGUILayout.PrefixLabel("Offset");
                            offset = EditorGUILayout.Vector2Field(string.Empty, offset, GUILayout.MaxWidth(128));
                        }

                        tilingProp.vectorValue = new Vector4(tiling.x, tiling.y, offset.x, offset.y);
                    }

                    fieldInfo.SetValue(prop, prop.flags ^ MaterialProperty.PropFlags.PerRendererData);
                    break;

                case MaterialProperty.PropType.Vector:
                    bool isTexTilingProp = false;

                    if (prop.name.EndsWith("_ST"))
                    {
                        string texName = prop.name.Remove(prop.name.Length - 3);

                        MaterialProperty texProperty = FindProperty(texName, properties);

                        if (texProperty != null) isTexTilingProp = true;
                    }

                    if (!isTexTilingProp) prop.vectorValue = materialEditor.VectorProperty(prop, prop.displayName);

                    break;
            }
        }
    }
}
