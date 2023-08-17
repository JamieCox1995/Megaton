using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameObjectReplace : EditorWindow
{
    private GameObject _prefab;

    [MenuItem("Window/GameObject Replace")]
    private static void Init()
    {
        var instance = GetWindow<GameObjectReplace>();

        instance.Show();
    }

    private void OnEnable()
    {
        Selection.selectionChanged += Repaint;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
    }

    private void OnGUI()
    {
        GameObject[] selection = Selection.objects.OfType<GameObject>().ToArray();

        string info = string.Format("{0} items selected to replace.", selection.Length);

        EditorGUILayout.HelpBox(info, MessageType.Info);

        _prefab = (GameObject)EditorGUILayout.ObjectField("Replace With:", _prefab, typeof(GameObject), false);


        GUI.enabled = (_prefab != null && selection != null && selection.Length > 0);

        if (GUILayout.Button("Replace"))
        {
            for (int i = 0; i < selection.Length; i++)
            {
                GameObject obj = selection[i];

                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);

                newObj.transform.parent = obj.transform.parent;
                newObj.transform.localPosition = obj.transform.localPosition;
                newObj.transform.localRotation = obj.transform.localRotation;
                newObj.transform.localScale = obj.transform.localScale;

                selection[i] = newObj;

                DestroyImmediate(obj);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        GUI.enabled = true;
    }
}
