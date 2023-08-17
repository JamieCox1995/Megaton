using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using System;

public class CinemachineController : MonoBehaviour
{
    public CinemachineBrain brain;

    [Header("Targeting Camera:")]
    public CinemachineFreeLook targetingCamera;
    private bool needsReset = false;

    [Header("Projectile Follow Camera:")]
    public CinemachineFreeLook followCamera;

    [Header("Scene Free Look Camera:")]
    public CinemachineVirtualCamera freeLookCamera;
    public float moveSpeed = 15f;

    [Header("Cinematic Camera")]
    public CinemachineClearShot cinematicClearShot;

    [Header("Animator Settings:")]
    public Animator m_Anim;

    private CinemachineState currentState = CinemachineState.Default;
    private CinemachineState previousState, nextState;

    private GameObject trackedGameObject;
    public Transform focusLocation;

    private bool stateQueued = false, stateLocked = false, shouldLockState = false, rotatePov = false;
    private bool useAlternateFreeLook = false;

	// Use this for initialization
	void Start ()
    {
        RegisterEvents();

        useAlternateFreeLook = SettingsManager._instance.gameSettings.alternativeCameraControls;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (trackedGameObject != null)
        {
            focusLocation.position = trackedGameObject.transform.position;
            focusLocation.rotation = trackedGameObject.transform.rotation;
        }

        if ((currentState == CinemachineState.Follow || currentState == CinemachineState.FreeLook) && ServiceLocator.Get<IInputProxyService>().GetKeyDown(KeyCode.G))
        {
            cinematicClearShot.LookAt = trackedGameObject.transform;

            QueueStateChange(CinemachineState.Cinematic);
        }
        else if (currentState == CinemachineState.Cinematic && ServiceLocator.Get<IInputProxyService>().GetKeyDown(KeyCode.G))
        {
            ReturnToPreviousState();
        }

        if (currentState == CinemachineState.Targeting)
        {
            bool attemptRotation = (useAlternateFreeLook && ServiceLocator.Get<IInputProxyService>().GetButton("FreeLook"))
                || (!useAlternateFreeLook && ServiceLocator.Get<IInputProxyService>().GetAxis("Target Camera Rotation") != 0f);

            if (attemptRotation)
            {
                StopAllCoroutines();

                targetingCamera.m_RecenterToTargetHeading.m_enabled = true;
                targetingCamera.m_XAxis.m_InputAxisValue = GetTargetRotationInput();
                needsReset = true;
            }
            else
            {
                targetingCamera.m_XAxis.m_InputAxisValue = 0f;
            }

            if (needsReset && attemptRotation == false)
            {
                StartCoroutine(ResetTargetingCamera(targetingCamera));
            }

        }

		if (currentState == CinemachineState.Follow)
        {
            followCamera.m_XAxis.m_InputAxisValue = ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Vertical");
            followCamera.m_YAxis.m_InputAxisValue = ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Horizontal");
        }

        if (currentState == CinemachineState.FreeLook)
        {
            /*if (rotatePov)
            {
                CinemachinePOV pov1 = freeLookCamera.GetCinemachineComponent<CinemachinePOV>();

                Vector3 lookDir = focusLocation.position - freeLookCamera.VirtualCameraGameObject.transform.position;
                Quaternion lookRot = Quaternion.FromToRotation(freeLookCamera.VirtualCameraGameObject.transform.forward, lookDir);

                //Quaternion lookRot = Quaternion.LookRotation(lookDir);

                pov1.m_HorizontalAxis.Value = (lookRot.y * Mathf.Rad2Deg);
                pov1.m_VerticalAxis.Value = lookRot.x * Mathf.Rad2Deg;

                Debug.LogError("Trying to look at the explosion position");

                Debug.DrawRay(freeLookCamera.VirtualCameraGameObject.transform.position, lookDir.normalized, Color.blue);

                Debug.DrawRay(freeLookCamera.VirtualCameraGameObject.transform.position, freeLookCamera.VirtualCameraGameObject.transform.forward * 2f, Color.red);

                rotatePov = false;
            }*/

            Vector3 moveVector = Vector3.zero;
            moveVector.x = ServiceLocator.Get<IInputProxyService>().GetAxis("Horizontal");
            moveVector.z = ServiceLocator.Get<IInputProxyService>().GetAxis("Vertical");

            moveVector = moveVector.normalized * moveSpeed * Time.unscaledDeltaTime;

            CinemachinePOV pov = freeLookCamera.GetCinemachineComponent<CinemachinePOV>();

            pov.m_HorizontalAxis.m_InputAxisValue = ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Vertical");
            pov.m_VerticalAxis.m_InputAxisValue = ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Horizontal");

            Quaternion rotation = Quaternion.Euler(pov.m_VerticalAxis.Value, pov.m_HorizontalAxis.Value, 0f);
            moveVector = rotation * moveVector;

            //freeLookFollowTarget.transform.position = freeLookFollowTarget.transform.forward + (moveVector.normalized * moveSpeed);
            focusLocation.position += moveVector;
        }
	}

    private IEnumerator ResetTargetingCamera(CinemachineFreeLook camera)
    {
        needsReset = false;

        yield return new WaitForSeconds(camera.m_RecenterToTargetHeading.m_RecenterWaitTime);

        float timer = 0f;

        while (timer <= camera.m_RecenterToTargetHeading.m_RecenteringTime)
        {
            timer += Time.deltaTime;

            float t = (timer / camera.m_RecenterToTargetHeading.m_RecenteringTime);

            camera.m_XAxis.Value = Mathf.Lerp(camera.m_XAxis.Value, 0f, t);

            yield return null;
        }

        camera.m_XAxis.Value = 0f;
    }

    private float GetTargetRotationInput()
    {
        float input = 0f;

        if (useAlternateFreeLook)
        {
            if (ServiceLocator.Get<IInputProxyService>().GetButton("FreeLook")) input = ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Vertical");
        }
        else
        {
            input = -ServiceLocator.Get<IInputProxyService>().GetAxis("Target Camera Rotation");
        }

        return input;
    }

    private void LateUpdate()
    {
        if (shouldLockState)
        {
            stateLocked = true;
            shouldLockState = false;
        }

        UpdateState();
    }

    private void UpdateState()
    {
        if (stateLocked == false)
        {
            if (stateQueued == true)
            {
                previousState = currentState;
                currentState = nextState;

                m_Anim.SetTrigger(currentState.ToString());

                stateQueued = false;
            }
        }
    }

    private void QueueStateChange(CinemachineState state)
    {
        stateQueued = true;
        nextState = state;
    }

    private void RegisterEvents()
    {
        // Here we are listening for the OnPlayerReady method, which is called when the player presses any key to begin with
        GameEventManager.instance.onPlayerReady += OnPlayerReady;
        GameEventManager.instance.onGamePaused += OnGamePaused;
        GameEventManager.instance.onGameUnpaused += OnGameUnPaused;

        GameEventManager.instance.onMortarFire += OnMortarFired;
        GameEventManager.instance.onProjectileDestroyed += OnProjectileDestoryed;
        GameEventManager.instance.onTurnEnd += OnTurnEnd;

        GameEventManager.instance.onLevelEventStarted += OnLevelEventStart;
        GameEventManager.instance.onLevelEventEnded += OnLevelEventEnd;

        GameEventManager.instance.onSettingsChanged += OnGameSaved;
    }

    // Called when the player starts the level by pressing any key. We want to enter the "Targeting" State
    private void OnPlayerReady()
    {
        QueueStateChange(CinemachineState.Targeting);
    }

    // Called when the game is paused. We want to enter the "Default" State
    private void OnGamePaused()
    {
        // Here we DO NOT want to queue the state change, as we want the player to be able to pause the game
        // in the middle of a level event.
        // QueueStateChange(CinemachineState.Default);
        previousState = currentState;
        currentState = CinemachineState.Default;

        SetAnimatorState("Default");
    }

    private void OnGameUnPaused()
    {
        // We also don't want to queue a state change here. We want to go back to the previous state.
        ReturnToPreviousState();
    }

    private void ReturnToPreviousState() 
    {
        CinemachineState stateBeforePause = previousState;
        previousState = currentState;

        switch (stateBeforePause)
        {
            case CinemachineState.Targeting:
                currentState = CinemachineState.Targeting;
                SetAnimatorState("Targeting");
                break;
            case CinemachineState.Follow:
                currentState = CinemachineState.Follow;
                SetAnimatorState("Follow");
                break;
            case CinemachineState.FreeLook:
                currentState = CinemachineState.FreeLook;
                SetAnimatorState("FreeLook");
                break;
            case CinemachineState.LevelEvent:
                currentState = CinemachineState.LevelEvent;
                SetAnimatorState("LevelEvent");
                break;
            case CinemachineState.Cinematic:
                currentState = CinemachineState.Cinematic;
                SetAnimatorState("Cinematic");
                break;
        }
    }

    private void OnMortarFired(MortarFireEvent fireEvent)
    {
        trackedGameObject = fireEvent.projectile.gameObject;

        followCamera.Follow = fireEvent.projectile.transform;
        followCamera.LookAt = fireEvent.projectile.transform;

        // We probably want to do a little bit of maths to make the camera be behind the projectile when it fires.
        followCamera.m_XAxis.Value = 0.5f;
        followCamera.m_YAxis.Value = 0.5f;

        QueueStateChange(CinemachineState.Follow);
    }

    private void OnProjectileDestoryed()
    {
        trackedGameObject = null;

        freeLookCamera.VirtualCameraGameObject.transform.position = followCamera.transform.position;

        Vector3 cameraBaseDirection = Vector3.ProjectOnPlane(focusLocation.forward, Vector3.up);

        float y = Vector3.Project(focusLocation.forward, Vector3.up).y;
        float verticalAngle = Mathf.Asin(-y) * Mathf.Rad2Deg;

        float x = cameraBaseDirection.x;
        float z = cameraBaseDirection.z;
        float horizontalAngle = Mathf.Atan2(x, z) * Mathf.Rad2Deg;

        CinemachinePOV pov1 = freeLookCamera.GetCinemachineComponent<CinemachinePOV>();

        pov1.m_HorizontalAxis.Value = horizontalAngle;
        pov1.m_VerticalAxis.Value = verticalAngle;

        if (currentState != CinemachineState.Cinematic)
        {
            QueueStateChange(CinemachineState.FreeLook);
        }
        else
        {
            previousState = CinemachineState.FreeLook;
            cinematicClearShot.LookAt = focusLocation;
        }
    }

    private void OnTurnEnd()
    {
        QueueStateChange(CinemachineState.Targeting);
    }

    private void OnLevelEventStart()
    {
        //QueueStateChange(CinemachineState.LevelEvent);

        previousState = currentState;
        currentState = CinemachineState.LevelEvent;

        SetAnimatorState("LevelEvent");

        shouldLockState = true;

        //StartCoroutine(LockStateNextFrame());
    }

    private void OnLevelEventEnd()
    {
        stateLocked = false;

        // Checking to see if a state has been queued during the level event
        if (stateQueued == false)
        {
            QueueStateChange(previousState);
        }
    }

    private IEnumerator LockStateNextFrame()
    {
        yield return null;

        stateLocked = true;
    }

    private void SetAnimatorState(string state)
    {
        m_Anim.SetTrigger(state);
    }

    private void OnGameSaved()
    {
        useAlternateFreeLook = SettingsManager._instance.gameSettings.alternativeCameraControls;
    }
}

[System.Serializable]
public enum CinemachineState
{
    Default, Targeting, Follow, Cinematic, FreeLook, LevelEvent
}
