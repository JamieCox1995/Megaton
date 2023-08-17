using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ObjectDepenetrator : ScriptableWizard
{
    public float MaxDistance = 0.01f;

    [MenuItem("Window/Object Depenetrator")]
    private static void Initialize()
    {
        ObjectDepenetrator wizard = ScriptableWizard.DisplayWizard<ObjectDepenetrator>("Depenetrate Objects", "Apply");
        wizard.Show();
    }

    //private void OnWizardUpdate()
    //{
    //    var objs = Selection.objects;

    //    helpString = string.Format("{0} object{1} selected", objs.Length, objs.Length == 1 ? "" : "s");
    //}

    private void OnGUI()
    {
        var objs = Selection.objects;

        var helpString = string.Format("{0} object{1} selected", objs.Length, objs.Length == 1 ? "" : "s");

        EditorGUILayout.HelpBox(helpString, MessageType.None);

        this.MaxDistance = EditorGUILayout.FloatField("Max Distance", this.MaxDistance);

        if (GUILayout.Button("Apply"))
        {
            OnWizardCreate();
            Close();
        }
        else
        {
            Repaint();
        }
    }

    private void OnWizardCreate()
    {
        var selected = Selection.objects;
        var selectedCount = selected.Length;
        var gameObjects = selected.Cast<GameObject>();

        Vector3 center = gameObjects.Aggregate(Vector3.zero, (v, o) => v += o.transform.position) / selectedCount;

        foreach (var obj in gameObjects)
        {
            Vector3 position = obj.transform.position;
            Vector3 direction = position - center;

            obj.transform.position = position + direction.normalized * this.MaxDistance;
        }
    }
}
