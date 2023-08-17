using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConstructionSiteBuilder))]
public class ConstructionZoneEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ConstructionSiteBuilder t = (ConstructionSiteBuilder)target;

        if (GUILayout.Button("Generate Construction Zone"))
        {
            t.Generate();
        }

        if (GUILayout.Button("Spawn Props"))
        {
            t.SpawnProps();
        }
    }
}
