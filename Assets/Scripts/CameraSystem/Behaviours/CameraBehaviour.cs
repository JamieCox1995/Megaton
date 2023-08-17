using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

namespace TotalDistraction.CameraSystem.Behaviours
{
    public abstract class CameraBehaviour : ScriptableObject
    {
        public abstract void UpdateBehaviour(Camera camera, GameObject target);
        public abstract void EndBehaviour(Camera camera);
        public abstract void StartBehaviour(Camera camera);

        protected static Quaternion InputRotation(Quaternion rotation, Vector2 inputSensitivity)
        {
            float xRotation = ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Horizontal") * inputSensitivity.x;
            float yRotation = ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Vertical") * inputSensitivity.y;

            Vector3 cameraRotation = rotation.eulerAngles;

            if (cameraRotation.x > 180f) cameraRotation.x -= 360f;

            float x = Mathf.Clamp(cameraRotation.x - xRotation, -90f, 90f);
            float y = cameraRotation.y + yRotation;

            Quaternion result = Quaternion.Euler(new Vector3(x, y, 0f));

            return result;
        }

        protected static Vector2 GetMouseSensitivity()
        {
            return new Vector2
            {
                x = SettingsManager._instance.gameSettings.mouseXSensitivity,
                y = SettingsManager._instance.gameSettings.mouseYSensitivity
            };
        }

        protected static bool UseAlternateCameraControls
        {
            get
            {
                return SettingsManager._instance.gameSettings.alternativeCameraControls;
            }
        }
    }
}