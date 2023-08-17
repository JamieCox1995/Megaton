using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingGenerator))]
public class BuildingGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Building"))
        {
            ((BuildingGenerator) serializedObject.targetObject).Generate();
        }

        if (serializedObject.FindProperty("_gameObjects").arraySize == 0) GUI.enabled = false;

        if (GUILayout.Button("Clear Building Pieces"))
        {
            ((BuildingGenerator)serializedObject.targetObject).Clear();
        }
        GUILayout.EndHorizontal();
    }

}
