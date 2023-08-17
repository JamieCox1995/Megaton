using UnityEngine;
using UnityEditor;
using TotalDistraction.CameraSystem.Animation;
using UnityEditor.Animations;
using System;

namespace TotalDistraction.CameraSystem.Editor
{
    [CustomEditor(typeof(CameraSystemController), true)]
    public class CameraSystemControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CameraSystemController cameraSystemAnimator = target as CameraSystemController;
            if (cameraSystemAnimator.Controller == null && GUILayout.Button("Create Camera System Controller"))
            {
                cameraSystemAnimator.Controller = CreateAnimatorController();
            }
        }

        private RuntimeAnimatorController CreateAnimatorController()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Camera System Controller", "Camera System Controller", "controller", string.Empty);
            if (string.IsNullOrEmpty(path)) return null;

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            var stateMachine = controller.layers[0].stateMachine;

            foreach (CameraMode cameraMode in Enum.GetValues(typeof(CameraMode)))
            {
                controller.AddParameter(string.Format("Trigger{0}", cameraMode), AnimatorControllerParameterType.Trigger);
                AnimatorState state = stateMachine.AddState(ObjectNames.NicifyVariableName(cameraMode.ToString()));
                UpdateCameraMode updateCameraMode = state.AddStateMachineBehaviour<UpdateCameraMode>();
                updateCameraMode.CameraMode = cameraMode;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = controller;

            return controller;
        }
    }
}
