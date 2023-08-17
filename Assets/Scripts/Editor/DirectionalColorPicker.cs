using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DirectionalColorPicker : EditorWindow
{
    [SerializeField]
    private Vector2 _vector;
    [SerializeField]
    private bool _autoCopy;

    private Rect _pickerRect;
    private Rect _swatchRect;

    private Texture _colorTexture;

    [MenuItem("Window/Directional Color Picker")]
    private static void Init()
    {
        DirectionalColorPicker picker = GetWindow<DirectionalColorPicker>(utility: true, title: "Directional Color Picker", focus: true);

        picker.Show();
    }

    private void OnEnable()
    {
        _colorTexture = GetColorTexture();
    }

    private void OnGUI()
    {
        _autoCopy = EditorGUILayout.Toggle("Auto-copy Hex", _autoCopy);
        _vector = EditorGUILayout.Vector2Field("Vector", _vector);

        if (_vector.magnitude > 1) _vector.Normalize();

        if (Event.current.type == EventType.Repaint)
        {
            _pickerRect = GUILayoutUtility.GetAspectRect(1);
        }
        else
        {
            GUILayoutUtility.GetRect(32, float.MaxValue, 32, float.MaxValue);
        }

        _vector = DrawVectorPicker(_pickerRect, _vector);

        Color color = ToNormalColor(_vector);

        EditorGUILayout.ColorField(new GUIContent("Color"), color, false, false, false, null);
        EditorGUILayout.TextField("Hex", ColorUtility.ToHtmlStringRGB(color));

        if (_autoCopy)
        {
            EditorGUIUtility.systemCopyBuffer = ColorUtility.ToHtmlStringRGB(color);
        }
    }

    private Color ToNormalColor(Vector2 v)
    {
        float z;
        if (v.sqrMagnitude >= 1)
        {
            z = 0;
        }
        else
        {
            float zSq = 1 - v.sqrMagnitude;

            z = Mathf.Sqrt(zSq);
        }

        Vector3 normal = new Vector3(v.x, v.y, z);

        return new Color(normal.x * 0.5f + 0.5f,
                         normal.y * 0.5f + 0.5f,
                         normal.z * 0.5f + 0.5f);
    }

    private Vector2 DrawVectorPicker(Rect r, Vector2 v)
    {
        int offset = 8;
        Rect inner = new RectOffset(offset, offset, offset, offset).Remove(r);

        float radius = Mathf.Min(inner.width, inner.height) / 2f;

        int xOffset = (int)(inner.width / 2f - radius);
        int yOffset = (int)(inner.height / 2f - radius);
        Rect textureRect = new RectOffset(xOffset, xOffset, yOffset, yOffset).Remove(inner);
        Graphics.DrawTexture(textureRect, _colorTexture);

        Vector2 endPoint = inner.center + v * radius;

        Handles.BeginGUI();
        Handles.color = EditorStyles.label.normal.textColor;
        Handles.DrawAAPolyLine(inner.center, endPoint);
        DrawAACircle(inner.center, radius);
        DrawAACircle(endPoint, 4f);
        Handles.EndGUI();

        EventType[] types = new[] { EventType.MouseDown, EventType.MouseDrag, EventType.MouseUp };

        if (types.Contains(Event.current.type) && r.Contains(Event.current.mousePosition))
        {
            Repaint();
            Vector2 result = (Event.current.mousePosition - inner.center) / radius;

            if (result.magnitude > 1) result.Normalize();

            return result;
        }
        else
        {
            return v;
        }
    }

    private Texture GetColorTexture()
    {
        const int width = 2048;
        const int height = 2048;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float py = y / (float)height;

            for (int x = 0; x < width; x++)
            {
                float px = x / (float)width;

                Vector2 p = new Vector2(2 * px - 1, 2 * -py + 1);

                if (p.magnitude > 1)
                {
                    pixels[y * width + x] = Color.clear;
                }
                else
                {
                    pixels[y * width + x] = ToNormalColor(p);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(false, true);

        return tex;
    }

    private void DrawAACircle(Vector2 center, float radius)
    {
        Vector3[] pts = new Vector3[256];

        for (int i = 0; i <= 255; i++)
        {
            float t = i / 255f;

            float x = center.x + radius * Mathf.Sin(t * Mathf.PI * 2);
            float y = center.y + radius * Mathf.Cos(t * Mathf.PI * 2);

            pts[i] = new Vector3(x, y, 0);
        }

        Handles.DrawAAPolyLine(pts);
    }
}
