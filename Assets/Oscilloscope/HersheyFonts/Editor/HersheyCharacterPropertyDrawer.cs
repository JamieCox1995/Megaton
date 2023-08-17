using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HersheyCharacter))]
public class HersheyCharacterPropertyDrawer : PropertyDrawer
{
    private const float CharacterPreviewHeight = 100f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float fullHeight = EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 4 + CharacterPreviewHeight;
        float minHeight = EditorGUIUtility.singleLineHeight;

        return property.isExpanded ? fullHeight : minHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Vector2 controlSize = new Vector2(position.width, EditorGUIUtility.singleLineHeight);

        int i = 0;

        Rect foldoutRect = new Rect(position.position, controlSize);
        Rect nameRect = new Rect(GetPositionFromIndex(position.position, ++i), controlSize);
        Rect glyphNoRect = new Rect(GetPositionFromIndex(position.position, ++i), controlSize);
        Rect boundsRect = new Rect(GetPositionFromIndex(position.position, ++i), controlSize);
        Rect previewRect = new Rect(GetPositionFromIndex(position.position, ++i), new Vector3(position.width, CharacterPreviewHeight));

        SerializedProperty nameProperty = property.FindPropertyRelative("Character");
        SerializedProperty glyphNoProperty = property.FindPropertyRelative("GlyphNumber");
        SerializedProperty boundsProperty = property.FindPropertyRelative("Bounds");

        bool isUnknownCharacter = string.IsNullOrEmpty(nameProperty.stringValue);

        GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);

        if (isUnknownCharacter)
        {
            label.text = string.Format("Unknown Character ({0})", glyphNoProperty.intValue);
            foldoutStyle.fontStyle = FontStyle.Bold;
        }
        else
        {
            label.text = string.Format("Character \"{0}\" ({1})", nameProperty.stringValue, glyphNoProperty.intValue);
        }



        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, foldoutStyle);

        if (property.isExpanded)
        {
            EditorGUI.PropertyField(nameRect, nameProperty);

            GUI.enabled = false;
            EditorGUI.PropertyField(glyphNoRect, glyphNoProperty);
            EditorGUI.PropertyField(boundsRect, boundsProperty);

            Color charColor = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label.normal.textColor;

            CharacterGUIRenderer.RenderStrokes(previewRect, property.FindPropertyRelative("Strokes"), charColor);
            GUI.enabled = true;
        }
    }

    private static Vector2 GetPositionFromIndex(Vector2 origin, int index)
    {
        return new Vector2(origin.x, origin.y + index * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
    }
}
