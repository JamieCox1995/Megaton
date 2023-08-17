using System;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

namespace TotalDistraction.CameraSystem.Behaviours
{
    public class FollowingCameraBehaviour : CameraBehaviour
    {
        public float ZoomSpeed;
        public float DefaultZoomDistance;
        public float MinimumZoomDistance;
        public float MaximumZoomDistance;
        public float MinimumCollisionClip;

        [NonSerialized]
        private bool _isInitialized;
        [NonSerialized]
        private float _distance;

        private bool _placeCameraBehindTarget;

        public override void EndBehaviour(Camera camera) { }

        public override void StartBehaviour(Camera camera)
        {
            if (!_isInitialized)
            {
                _distance = this.DefaultZoomDistance;
                _isInitialized = true;
            }

            _placeCameraBehindTarget = true;
        }

        public override void UpdateBehaviour(Camera camera, GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("FollowingCameraBehaviour has no target.");
                return;
            }

            Vector2 sensitivity = GetMouseSensitivity();

            Quaternion rotation;

            if (_placeCameraBehindTarget)
            {
                rotation = Quaternion.LookRotation(target.transform.forward, Vector3.up);
                _placeCameraBehindTarget = false;
            }
            else
            {
                rotation = InputRotation(camera.transform.rotation, sensitivity);
            }

            // Update the camera distance from the target
            float zoomInput = -1 * ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Zoom");
            float deltaDistance = zoomInput * this.ZoomSpeed;
            _distance = Mathf.Clamp(_distance + deltaDistance, this.MinimumZoomDistance, this.MaximumZoomDistance);

            Vector3 cameraTargetOffset = Vector3.back * _distance;
            Vector3 cameraOffsetDirection = rotation * cameraTargetOffset;
            Vector3 desiredCameraPosition = cameraOffsetDirection + target.transform.position;

            // Raycast before moving the camera to check for collisions
            RaycastHit hit;
            if (Physics.SphereCast(target.transform.position, 1f, cameraOffsetDirection.normalized, out hit, _distance))
            {
                float distanceToCollision = Vector3.Distance(target.transform.position, hit.point);

                if (distanceToCollision >= this.MinimumCollisionClip)
                {
                    cameraTargetOffset = Vector3.back * distanceToCollision;
                    cameraOffsetDirection = rotation * cameraTargetOffset;
                    desiredCameraPosition = cameraOffsetDirection + target.transform.position;
                }
            }

            camera.transform.position = desiredCameraPosition;
            camera.transform.rotation = rotation;
        }
    }
}
