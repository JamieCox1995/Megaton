using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public partial class BandedSkyboxGUI : ShaderGUI
{
    private ReorderableList _gradientUi;
    private ShaderGradient _gradient;
    private int _previousControlId;

    private Texture _texture;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        bool useGradient = material.shaderKeywords.Contains("USE_GRADIENT_TEXTURE");

        properties = Except(properties, "_Gradient");

        if (useGradient) properties = Except(properties, "_SkyColor1", "_SkyColor2");

        base.OnGUI(materialEditor, properties);

        if (useGradient)
        {
            if (_gradient == null) _gradient = FindGradient(material);
            if (_texture == null) _texture = FindTexture(_gradient, material);
            if (_gradientUi == null) _gradientUi = GetGradientUI(_gradient);

            EditorGUI.BeginChangeCheck();

            _gradientUi.DoLayoutList();

            bool keyboardFocusChanged = GUIUtility.keyboardControl != _previousControlId;
            bool editorChangedUsingMouse = GUIUtility.keyboardControl == 0 && EditorGUI.EndChangeCheck();

            if (keyboardFocusChanged || editorChangedUsingMouse)
            {
                UpdateMaterialGradient(material);
                AssetDatabase.SaveAssets();
            }

            _previousControlId = GUIUtility.keyboardControl;
        }
    }

    private void UpdateMaterialGradient(Material material)
    {
        _gradient.WriteToTexture(_texture);

        material.SetColor("_SkyColor1", _gradient.First().color);
        material.SetColor("_SkyColor2", _gradient.Last().color);
        material.SetTexture("_Gradient", _texture);
    }

    private ShaderGradient FindGradient(Material material)
    {
        //string materialPath = AssetDatabase.GetAssetPath(material);
        //string dir = Path.GetDirectoryName(materialPath);
        //string assetPath = string.Format("{0}/{1} Gradient.asset", dir, material.name);

        string assetPath = AssetDatabase.GetAssetPath(material);

        ShaderGradient g = AssetDatabase.LoadAssetAtPath<ShaderGradient>(assetPath);

        //DebugUtils.LogAssertionExpression(() => g != null);

        if (g == null)
        {
            Color start = material.GetColor("_SkyColor1");
            Color end = material.GetColor("_SkyColor2");

            g = ShaderGradient.Create(start, end);

            //AssetDatabase.CreateAsset(g, assetPath);
            AssetDatabase.AddObjectToAsset(g, assetPath);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(g));
        }

        return g;
    }

    private Texture FindTexture(ShaderGradient gradient, Material material)
    {
        string assetPath = AssetDatabase.GetAssetPath(material);

        Texture t = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);

        if (t == null)
        {
            t = gradient.ToTexture(512);

            material.SetTexture("_Gradient", t);

            AssetDatabase.AddObjectToAsset(t, assetPath);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(t));
        }

        return t;
    }

    private ReorderableList GetGradientUI(ShaderGradient gradient)
    {
        ReorderableList result = new ReorderableList(gradient, typeof(GradientStop), true, true, true, true);

        result.onCanRemoveCallback = list => list.count > 2;

        result.onAddCallback = list =>
        {
            int index = list.index <= 0 ? 1 : list.index;
            list.index = index;

            GradientStop previous = gradient[index - 1];
            GradientStop current = gradient[index];

            float averageT = Mathf.Lerp(previous.t, current.t, 0.5f);
            Color averageColor = Color.Lerp(previous.color, current.color, 0.5f);

            GradientStop newStop = new GradientStop { t = averageT, color = averageColor };

            gradient.Insert(index, newStop);
        };

        Func<float> draggedY = GetFloatField(result, "m_DraggedY");
        Func<float> dragOffset = GetFloatField(result, "m_DragOffset");
        Func<float> getListElementHeight = Get_GetListElementHeight(result);

        result.onReorderCallback = list =>
        {
            float dragPos = draggedY() - dragOffset();
            float listHeight = getListElementHeight() - (list.elementHeight + 7f);

            float t = dragPos / listHeight;

            GradientStop stop = gradient[list.index];
            stop.t = Mathf.Clamp01(t);
            gradient[list.index] = stop;
        };

        result.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Gradient");
        };

        result.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = gradient[index];

            rect.y += 2;

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(string.Format("GradientStop-T-{0}", index));
            element.t = Mathf.Clamp01(EditorGUI.FloatField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.t));
            GUI.SetNextControlName(string.Format("GradientStop-Color-{0}", index));
            element.color = EditorGUI.ColorField(new Rect(rect.x + 60, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                                                 new GUIContent(),
                                                 element.color,
                                                 true,
                                                 false,
                                                 true,
                                                 new ColorPickerHDRConfig(0, 8, 0.125f, 3));

            if (EditorGUI.EndChangeCheck()) gradient[index] = element;
        };

        return result;
    }

    private Func<float> Get_GetListElementHeight(ReorderableList instance)
    {
        System.Reflection.MethodInfo method = typeof(ReorderableList).GetMethod("GetListElementHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), instance, method);
    }

    private Func<float> GetFloatField(ReorderableList instance, string name)
    {
        System.Reflection.FieldInfo field = typeof(ReorderableList).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return () => (float)field.GetValue(instance);
    }

    private MaterialProperty[] Except(MaterialProperty[] properties, params string[] filterNames)
    {
        return properties.Where(prop => !filterNames.Contains(prop.name)).ToArray();
    }
}
