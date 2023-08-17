using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HersheyFont))]
public class HersheyFontEditor : Editor
{
    private SerializedProperty _charactersProperty;

    private void OnEnable()
    {
        _charactersProperty = serializedObject.FindProperty("Characters");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        int charCount = _charactersProperty.arraySize;

        GUILayout.BeginHorizontal();

        bool expandAll = GUILayout.Button("Expand all");
        bool collapseAll = GUILayout.Button("Collapse all");

        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField(string.Format("{0} character{1}", charCount, charCount == 1 ? string.Empty : "s"));

        for (int i = 0; i < charCount; i++)
        {
            var characterProperty = _charactersProperty.GetArrayElementAtIndex(i);

            if (expandAll)
            {
                characterProperty.isExpanded = true;
            }
            if (collapseAll)
            {
                characterProperty.isExpanded = false;
            }

            EditorGUILayout.PropertyField(characterProperty);
        }

        serializedObject.ApplyModifiedProperties();
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        Color color = Color.white;

        CharacterGUIRenderer.RenderStrokes(r, _charactersProperty.GetArrayElementAtIndex(20).FindPropertyRelative("Strokes"), color);
    }
}
