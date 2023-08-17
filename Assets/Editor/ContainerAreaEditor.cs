using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShippingContainerSpawner))]
public class ContainerAreaEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShippingContainerSpawner spawner = (ShippingContainerSpawner)target;

        GUILayout.BeginHorizontal("Spawn Buttons");
        #region Spawn Buttons
        if (GUILayout.Button("Spawn Containers"))
        {
            spawner.SpawnContainers();
        }

        if (GUILayout.Button("Spawn Rails"))
        {
            spawner.SpawnGuideRails();
        }

        if (GUILayout.Button("Spawn All"))
        {
            spawner.CreateContainerArea();
        }
        #endregion
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Clear buttons");
        if (GUILayout.Button("Clear Containers"))
        {
            spawner.ClearContainers();
        }
        if (GUILayout.Button("Clear Rails"))
        {
            spawner.ClearRailGuides();
        }
        if (GUILayout.Button("Clear All"))
        {
            spawner.ClearArea();
        }
        GUILayout.EndHorizontal();
    }
}
