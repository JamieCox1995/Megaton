using System;
using UnityEngine;

namespace TotalDistraction.CameraSystem.Behaviours
{
    public class DefaultCameraBehaviour : CameraBehaviour
    {
        public Vector3 Focus;
        public float HorizontalOffset;
        public float VerticalOffset;
        public float RotationTime;

        [NonSerialized]
        private float _currentRotation;

        public override void UpdateBehaviour(Camera camera, GameObject target)
        {
            _currentRotation += 360f * Time.unscaledDeltaTime / this.RotationTime;
            Quaternion offsetRotation = Quaternion.Euler(0f, _currentRotation, 0f);

            Vector3 offset = Vector3.back * this.HorizontalOffset + Vector3.up * this.VerticalOffset;

            Vector3 cameraRelativePosition = offsetRotation * offset;

            Vector3 cameraPosition = this.Focus + cameraRelativePosition;

            Quaternion cameraRotation = Quaternion.LookRotation(-cameraRelativePosition, Vector3.up);

            camera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
        }

        public override void StartBehaviour(Camera camera) { }
        public override void EndBehaviour(Camera camera) { }
    }
}
