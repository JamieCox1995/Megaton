using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

namespace TotalDistraction.CameraSystem.Behaviours
{
    public class FreeCameraBehaviour : CameraBehaviour
    {
        public float MovementSpeed;
        public float CameraCollisionDistance;

        public override void UpdateBehaviour(Camera camera, GameObject target)
        {
            Vector2 sensitivity = GetMouseSensitivity();

            float xMovement = ServiceLocator.Get<IInputProxyService>().GetAxis("Vertical") * this.MovementSpeed * Time.unscaledDeltaTime;
            float yMovement = ServiceLocator.Get<IInputProxyService>().GetAxis("Horizontal") * this.MovementSpeed * Time.unscaledDeltaTime;

            Vector3 movement = new Vector3(yMovement, 0f, xMovement);

            movement = camera.transform.rotation * movement;

            // Raycast before moving the camera to check for collisions
            Vector3 direction = movement.normalized;

            if (movement.sqrMagnitude > 0)
            {
                // The camera can tunnel through things without additional padding of the raycast distance
                float distance = movement.magnitude + this.CameraCollisionDistance;

                if (!Physics.Raycast(camera.transform.position, direction, distance))
                {
                    camera.transform.position += movement;
                }
            }

            camera.transform.rotation = InputRotation(camera.transform.rotation, sensitivity);
        }

        public override void StartBehaviour(Camera camera) { }
        public override void EndBehaviour(Camera camera) { }
    }
}
