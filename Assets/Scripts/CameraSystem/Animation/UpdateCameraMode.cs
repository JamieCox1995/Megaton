using System;
using UnityEngine;

namespace TotalDistraction.CameraSystem.Animation
{

    public class UpdateCameraMode : StateMachineBehaviour
    {
        public CameraMode CameraMode;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (CameraMode mode in Enum.GetValues(typeof(CameraMode)))
            {
                animator.ResetTrigger(string.Format("Trigger{0}", mode));
            }

            CameraSystemController cameraSystemAnimator = animator.GetComponent<CameraSystemController>();

            if (cameraSystemAnimator == null)
            {
                Debug.LogError("No CameraSystemAnimator component attached to this Animator.");
            }
            else
            {
                cameraSystemAnimator.SetCameraMode(this.CameraMode);
            }
        }
    }
}
