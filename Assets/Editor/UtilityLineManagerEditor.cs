using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UtilityLineManager))]
public class UtilityLineManagerEditor : Editor
{
    SerializedProperty _utilityPoleInstanceProp;
    SerializedProperty _connectorsProp;
    SerializedProperty _utilityLinesProp;

    private void OnEnable()
    {
        _utilityPoleInstanceProp = serializedObject.FindProperty("UtilityPoleInstance");
        _connectorsProp = serializedObject.FindProperty("Connectors");
        _utilityLinesProp = serializedObject.FindProperty("UtilityLines");
    }

    private void OnSceneGUI()
    {
        UtilityLineManager manager = target as UtilityLineManager;

        if (manager == null) return;

        EditorGUI.BeginChangeCheck();
        if (manager.Connectors != null)
        {
            foreach (UtilityLineManager.UtilityConnector connector in manager.Connectors)
            {
                connector.Position = Handles.PositionHandle(connector.Position, Quaternion.identity);
                Handles.Label(connector.Position, string.Format("ID: {0}\nType: {1}", connector.Id, connector.Type));
            }
        }

        if (manager.UtilityLines != null)
        {
            int i = 0;
            foreach (UtilityLineManager.UtilityLine line in manager.UtilityLines)
            {
                line.Position = Handles.PositionHandle(line.Position, Quaternion.Euler(line.Rotation));
                line.Rotation = Handles.RotationHandle(Quaternion.Euler(line.Rotation), line.Position).eulerAngles;
                Handles.Label(line.Position, string.Format("ID: {0}", i++));
                
                if (line.Previous != -1)
                {
                    UtilityLineManager.UtilityLine previous = manager.UtilityLines[line.Previous];

                    Handles.DrawLine(line.Position, previous.Position);
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            manager.Refresh();
        }
    }
}
