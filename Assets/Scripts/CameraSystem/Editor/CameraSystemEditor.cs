using System;
using System.Linq;
using TotalDistraction.CameraSystem.Behaviours;
using UnityEditor;
using UnityEngine;

namespace TotalDistraction.CameraSystem.Editor
{
    [CustomEditor(typeof(CameraSystem))]
    public class CameraSystemEditor : UnityEditor.Editor
    {
        private Rect _dropdownRect;

        public override void OnInspectorGUI()
        {
            CameraSystem cameraSystem = target as CameraSystem;
            foreach (CameraMode key in Enum.GetValues(typeof(CameraMode)))
            {
                CameraBehaviour value = cameraSystem[key];

                EditorGUI.BeginChangeCheck();
                value = (CameraBehaviour)EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(key.ToString()), value, typeof(CameraBehaviour), false);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cameraSystem, "Attach Camera Behaviour to Camera System");
                    cameraSystem.Register(key, value);
                }
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Create new Camera Behaviour"), FocusType.Keyboard))
            {
                
                GenericMenu menu = new GenericMenu();

                foreach (Type type in GetCameraBehaviourTypes())
                {
                    string typeName = ObjectNames.NicifyVariableName(type.Name);
                    menu.AddItem(new GUIContent(typeName), false, () => CreateNewAsset(type));
                }

                menu.DropDown(_dropdownRect);
            }
            else if (Event.current.type == EventType.Repaint)
            {
                _dropdownRect = GUILayoutUtility.GetLastRect();
            }
        }

        private Type[] GetCameraBehaviourTypes()
        {
            MonoScript[] allScripts = Resources.FindObjectsOfTypeAll<MonoScript>();

            return allScripts.Select(script => script.GetClass()).Where(type => typeof(CameraBehaviour).IsAssignableFrom(type) && type.IsAbstract == false).ToArray();
        }

        private void CreateNewAsset(Type assetType)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(assetType)) throw new ArgumentException("assetType must be derived from ScriptableObject.", "assetType");

            ScriptableObject o = ScriptableObject.CreateInstance(assetType);

            string name = string.Format("New {0}.asset", assetType.Name);

            ProjectWindowUtil.CreateAsset(o, name);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }
    }
}
