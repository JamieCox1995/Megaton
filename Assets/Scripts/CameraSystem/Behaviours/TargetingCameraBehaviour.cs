using System;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

namespace TotalDistraction.CameraSystem.Behaviours
{
    public class TargetingCameraBehaviour : CameraBehaviour
    {
        [Header("Rotation Settings")]
        public float RotationSpeed;
        public float RotationResetTime;
        public AnimationCurve RotationResetCurve;
        [Header("Position Settings")]
        public float HorizontalOffset;
        public float ZoomSpeed;
        public float DefaultHeight;
        public float MinimumHeight;
        public float MaximumHeight;
        public PidController3 MovementController;

        [NonSerialized]
        private bool _isInitialized;
        [NonSerialized]
        private float _cameraRotation;
        [NonSerialized]
        private float _cameraHeight;
        [NonSerialized]
        private float _rotationResetAngle;
        [NonSerialized]
        private float _rotationResetTime;
        [NonSerialized]
        private bool _needsReset;
        [NonSerialized]
        private Vector3 _mortarPosition;

        public override void EndBehaviour(Camera camera) { }

        public override void StartBehaviour(Camera camera)
        {
            if (!_isInitialized)
            {
                _cameraHeight = this.DefaultHeight;
                _isInitialized = true;

                Mortar mortar = GameObject.FindObjectOfType<Mortar>();
                if (mortar != null)
                {
                    _mortarPosition = mortar.transform.position;
                }
                else
                {
                    Debug.LogWarning("Could not find Mortar in scene.");
                }
            }
        }

        public override void UpdateBehaviour(Camera camera, GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("TargetingCameraBehaviour has no target.");
                return;
            }

            bool isPlayerRotating = (UseAlternateCameraControls && ServiceLocator.Get<IInputProxyService>().GetButton("FreeLook"))
                || (!UseAlternateCameraControls && ServiceLocator.Get<IInputProxyService>().GetAxis("Target Camera Rotation") != 0);

            if (isPlayerRotating)
            {
                _cameraRotation = _cameraRotation + GetRotationInput();
                _rotationResetAngle = ConstrainAngle(_cameraRotation);
                if (!_needsReset) _needsReset = true;
            }
            else if (_needsReset)
            {
                // smoothly rotate back to the initial rotation
                _rotationResetTime += Time.deltaTime;
                float t = _rotationResetTime / this.RotationResetTime;
                _cameraRotation = _rotationResetAngle * this.RotationResetCurve.Evaluate(t);

                if (t >= 1)
                {
                    _rotationResetAngle = 0f;
                    _rotationResetTime = 0f;
                    _needsReset = false;
                }
            }
            
            _cameraHeight = GetCameraHeight();

            // get the direction along the horizontal plane from the target to the mortar
            Vector3 mortarDirection = Vector3.ProjectOnPlane(target.transform.position - _mortarPosition, Vector3.up).normalized;

            Quaternion toBehindTarget = Quaternion.FromToRotation(Vector3.forward, mortarDirection);
            Quaternion cameraRotation = Quaternion.Euler(0f, toBehindTarget.eulerAngles.y + _cameraRotation, 0f);
            Vector3 cameraOffset = Vector3.back * this.HorizontalOffset + Vector3.up * _cameraHeight;

            Vector3 cameraOffsetDirection = cameraRotation * cameraOffset;
            Vector3 cameraPosition = cameraOffsetDirection + target.transform.position;

            Vector3 cameraError = cameraPosition - camera.transform.position;
            Vector3 positionOutput = this.MovementController.GetOutput(cameraError, Time.unscaledDeltaTime);

            camera.transform.rotation = Quaternion.LookRotation(-cameraOffsetDirection, Vector3.up);
            camera.transform.position += positionOutput;
        }

        private float GetRotationInput()
        {
            float result = 0f;

            if (CameraBehaviour.UseAlternateCameraControls)
            {
                if (ServiceLocator.Get<IInputProxyService>().GetButton("FreeLook")) result = ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Vertical");
            }
            else
            {
                result = ServiceLocator.Get<IInputProxyService>().GetAxis("Target Camera Rotation") * this.RotationSpeed * Time.deltaTime;
            }

            return result;
        }

        private float GetCameraHeight()
        {
            float height = _cameraHeight - ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Zoom") * this.ZoomSpeed;

            height = Mathf.Clamp(height, this.MinimumHeight, this.MaximumHeight);

            return height;
        }

        private static float ConstrainAngle(float angle)
        {
            // maps any angle to [-180, 180]
            return angle < 0f ? ((angle - 180f) % 360f) + 180f : ((angle + 180f) % 360f) - 180f;
        }
    }
}
