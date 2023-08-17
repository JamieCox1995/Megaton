using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CharacterGUIRenderer
{
    public static void RenderStrokes(Rect rect, SerializedProperty property, Color color)
    {
        Color previousColor = Handles.color;
        Handles.color = color;
        for (int i = 0; i < property.arraySize; i++)
        {
            var stroke = property.GetArrayElementAtIndex(i);
            var strokePoints = stroke.FindPropertyRelative("Points");

            var points = new List<Vector2Int>();
            for (int j = 0; j < strokePoints.arraySize; j++)
            {
                var point = strokePoints.GetArrayElementAtIndex(j);

                var p = point.vector2IntValue;
                p.y = -p.y;

                points.Add(p);
            }

            var finalPoints = points.Select(p => (Vector2)p).Select(p => rect.center + 2 * p).Select(p => (Vector3)p);

            Handles.DrawAAPolyLine(finalPoints.ToArray());
        }

        Handles.color = previousColor;
    }
}