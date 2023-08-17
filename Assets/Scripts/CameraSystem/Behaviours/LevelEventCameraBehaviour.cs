using System;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace TotalDistraction.CameraSystem.Behaviours
{
    public class LevelEventCameraBehaviour : CameraBehaviour
    {
        public PostProcessingProfile LevelEventPostProcessingProfile;
        public Vector3 CameraPosition;
        public Vector3 CameraRotation;

        [NonSerialized]
        private PostProcessingBehaviour _postProcessingBehaviour;
        [NonSerialized]
        private PostProcessingProfile _overriddenPostProcessingProfile;

        public override void EndBehaviour(Camera camera)
        {
            if (_overriddenPostProcessingProfile == null)
            {
                Destroy(_postProcessingBehaviour);
            }
            else
            {
                _postProcessingBehaviour.profile = _overriddenPostProcessingProfile;
            }
        }

        public override void StartBehaviour(Camera camera)
        {
            _postProcessingBehaviour = camera.GetComponent<PostProcessingBehaviour>();

            if (_postProcessingBehaviour == null)
            {
                _postProcessingBehaviour = camera.gameObject.AddComponent<PostProcessingBehaviour>();
            }
            else
            {
                _overriddenPostProcessingProfile = _postProcessingBehaviour.profile;
            }

            _postProcessingBehaviour.profile = this.LevelEventPostProcessingProfile;
        }

        public override void UpdateBehaviour(Camera camera, GameObject target)
        {
            camera.transform.SetPositionAndRotation(this.CameraPosition, Quaternion.Euler(this.CameraRotation));
        }
    }
}
