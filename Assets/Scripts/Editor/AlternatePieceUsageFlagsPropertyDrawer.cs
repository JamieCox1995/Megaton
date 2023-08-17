using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(BuildingGenerator.AlternatePieceUsageFlags))]
public class AlternatePieceUsageFlagsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        property.intValue = (int)(BuildingGenerator.AlternatePieceUsageFlags)EditorGUI.EnumMaskField(position, property.displayName, (BuildingGenerator.AlternatePieceUsageFlags)property.intValue);
        EditorGUI.EndProperty();
    }
}