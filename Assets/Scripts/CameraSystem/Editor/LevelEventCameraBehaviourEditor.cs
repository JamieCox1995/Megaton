using UnityEngine;
using UnityEditor;
using TotalDistraction.CameraSystem.Behaviours;

namespace TotalDistraction.CameraSystem.Editor
{
    [CustomEditor(typeof(LevelEventCameraBehaviour))]
    public class LevelEventCameraBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LevelEventCameraBehaviour cameraBehaviour = target as LevelEventCameraBehaviour;

            if (GUILayout.Button("Copy from GameObject"))
            {
                EditorGUIUtility.ShowObjectPicker<GameObject>(null, true, "", 0);
            }
            else if (GUILayout.Button("Align With View"))
            {
                Transform sceneCameraTransform = SceneView.lastActiveSceneView.camera.transform;
                AlignCameraBehaviourToTransform(cameraBehaviour, sceneCameraTransform);
            }

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                GameObject gameObject = EditorGUIUtility.GetObjectPickerObject() as GameObject;

                if (gameObject != null) AlignCameraBehaviourToTransform(cameraBehaviour, gameObject.transform);
            }
        }

        private void AlignCameraBehaviourToTransform(LevelEventCameraBehaviour behaviour, Transform transform)
        {
            //Undo.RecordObject(behaviour, "Apply Position and Rotation to Camera Behaviour");
            Undo.RegisterCompleteObjectUndo(behaviour, "Apply Position and Rotation to Camera Behaviour");
            behaviour.CameraPosition = transform.position;
            behaviour.CameraRotation = transform.rotation.eulerAngles;
            Undo.FlushUndoRecordObjects();
            EditorUtility.SetDirty(behaviour);
        }
    }
}
