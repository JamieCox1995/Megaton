using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BuildingGenerator.PuzzlePieceFlags))]
public class PuzzlePieceFlagsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        property.intValue = (int)(BuildingGenerator.PuzzlePieceFlags)EditorGUI.EnumMaskField(position, property.displayName, (BuildingGenerator.PuzzlePieceFlags)property.intValue);
        EditorGUI.EndProperty();
    }
}
