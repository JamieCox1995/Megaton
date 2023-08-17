using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;

public class GameplayCamera : MonoBehaviour
{
    [SerializeField]
    private CameraSettings mortarSettings;
    [SerializeField]
    private CameraSettings projectileSettings;
    [SerializeField]
    private FreeCamSettings freeCam;

    private CameraSettings settingsToUse;

    [SerializeField]
    private GameObject target;

    [SerializeField]
    private RenderTexture targetTexture;

    private float sensitivityXMod;
    private float sensitivityYMod;
    private float currentX = 0f;
    private float currentY = 0f;
    private float initialX, initialY;

    private bool overrideControl;
    private bool onFinalShot = false;

    private void RegisterEvents()
    {
        GameEventManager.instance.onMortarFire += OnMortarFired;
        GameEventManager.instance.onAftershockPrimed += OnFinalShot;

        GameEventManager.instance.onTurnEnd += OnTurnEnd;

        GameEventManager.instance.onPlayerReady += OnPlayerReady;
        GameEventManager.instance.onPlayerUnready += OnPlayerUnready;

        GameEventManager.instance.onGamePaused += OnPlayerUnready;
        GameEventManager.instance.onGameUnpaused += OnPlayerReady;

        GameEventManager.instance.onLevelEventStarted += OnLevelEventStarted;
        GameEventManager.instance.onLevelEventEnded += OnLevelEventEnded;

        GameEventManager.instance.onGameOver += OnPlayerUnready;
    }

	// Use this for initialization
	void Start ()
    {
        overrideControl = false;

        target = GameObject.FindGameObjectWithTag("Player");
        settingsToUse = mortarSettings;

        currentX = initialX = target.transform.eulerAngles.y;
        currentY = initialY = target.transform.eulerAngles.x;

        OrbitTarget();

        transform.eulerAngles = new Vector3(currentY, currentX, 0f);

        SensitivityChanged();

        RegisterEvents();
    }

    // Update is called once per frame
    void Update()
    {
        SensitivityChanged();


        if (overrideControl == false)
        {
            if (target != null)
            {
                OrbitTarget();
            }
            else
            {
                FreeMovement();
            }
        }
	}

    public void SensitivityChanged()
    {
        sensitivityXMod = SettingsManager._instance.gameSettings.mouseXSensitivity;
        sensitivityYMod = SettingsManager._instance.gameSettings.mouseYSensitivity;
    }

    private void FreeMovement()
    {
        float xMove = ServiceLocator.Get<IInputProxyService>().GetAxis("Vertical") * freeCam.standardMoveSpeed * Time.unscaledDeltaTime;
        float yMove = ServiceLocator.Get<IInputProxyService>().GetAxis("Horizontal") * freeCam.standardMoveSpeed * Time.unscaledDeltaTime;

        Vector3 move = new Vector3(yMove, 0f, xMove);
        move = transform.rotation * move;

        // Before we move the camera, we want to fire a raycast to see if we are going to hit anything.
        RaycastHit hit;
        Vector3 dir = move.normalized;

        Debug.DrawRay(transform.position, dir, Color.blue, Time.deltaTime);

        if(!Physics.Raycast(transform.position, dir, out hit, move.magnitude * 2f))
        {
            transform.position += move;
        }

        currentX += ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Vertical") * (freeCam.mouseSpeed.x * sensitivityXMod);
        currentY -= ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Horizontal") * (freeCam.mouseSpeed.y * sensitivityYMod)* ((freeCam.invert == true) ? -1 : 1);

        currentY = Utilities.ValidateAngle(currentY);
        currentY = Mathf.Clamp(currentY, -90f, 90f);

        transform.eulerAngles = new Vector3(currentY, currentX, 0f);
    }

    private void OrbitTarget()
    {
        if (!settingsToUse.lockCameraBehindTarget)
        {
            CameraScroll();

            CameraMovement();
        }
        else
        {
            LookAtTargetDirection();
        }
    }

    private void CameraScroll()
    {
        float scrollInput = -ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Zoom");
        settingsToUse.distance += scrollInput * 2.5f;

        settingsToUse.distance = Mathf.Clamp(settingsToUse.distance, settingsToUse.minMaxDistance.x, settingsToUse.minMaxDistance.y);
    }

    private void CameraMovement()
    {
        currentX += ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Vertical") * (settingsToUse.cameraMoveSpeed.x * sensitivityXMod) * /*settingsToUse.distance */ Time.unscaledDeltaTime;
        currentY -= ServiceLocator.Get<IInputProxyService>().GetAxis("Camera Horizontal") * (settingsToUse.cameraMoveSpeed.y * sensitivityYMod) * Time.unscaledDeltaTime * ((settingsToUse.invert == true) ? -1 : 1);

        currentY = Utilities.ValidateAngle(currentY);
        currentY = Mathf.Clamp(currentY, -90f, 90f);

        Quaternion rot = Quaternion.Euler(currentY, currentX, 0f);
        Vector3 offset = new Vector3(0, settingsToUse.heightOffset, -settingsToUse.distance);

        Vector3 position = rot * offset + target.transform.position;

        // Here we want to check to see if the position we have calculated is colliding with anything
        RaycastHit hit;
        Vector3 directionToCheck = (position - target.transform.position).normalized;

        if (Physics.SphereCast(target.transform.position, 1f, directionToCheck, out hit, settingsToUse.distance))
        {
            float distanceToCollision = Vector3.Distance(target.transform.position, hit.point);

            // If the collision distance is greater than some min distance, we shall reduce the camera's orbit offset to the hit distance
            if (distanceToCollision >= projectileSettings.minCollisionClip)
            {
                offset = new Vector3(0, settingsToUse.heightOffset, -distanceToCollision);

                position = rot * offset + target.transform.position;
            }
        }

        transform.position = position;
        transform.rotation = rot;
    }

    private void LookAtTargetDirection()
    {
        Vector3 offset = new Vector3(0f, settingsToUse.heightOffset, -settingsToUse.distance);

        Quaternion rotation = Quaternion.Euler(0f, target.transform.localEulerAngles.y, 0f);

        Vector3 position = rotation * offset + target.transform.position;

        transform.position = position;
        transform.rotation = rotation;
    }

    public void OverrideTranform(Transform trans)
    {
        overrideControl = true;

        transform.position = trans.position;
        transform.rotation = trans.rotation;
    }

    public void CancelOverride()
    {
        overrideControl = false;

        //if (!onFinalShot)
            //OnTurnEnd();
    }

    private void OnLevelEventStarted()
    {

    }


    private void OnLevelEventEnded()
    {
        CancelOverride();
    }

    private void OnFinalShot()
    {
        onFinalShot = true;
    }

    public void SetOrbitCameraTarget(GameObject target)
    {
        settingsToUse = projectileSettings;
        this.target = target;
    }

    #region Methods for Events
    private void OnMortarFired(MortarFireEvent fireEvent)
    {
        GetComponent<Camera>().targetTexture = null;

        settingsToUse = projectileSettings;
        target = fireEvent.projectile;

        // Here we want to add the AudioListener to the object
        if (!GetComponent<AudioListener>())
        {
            gameObject.AddComponent<AudioListener>();
        }
    }

    private void OnTurnEnd()
    {
        if (!overrideControl && !onFinalShot)
        {
            GetComponent<Camera>().targetTexture = targetTexture;

            target = GameObject.FindGameObjectWithTag("Player");
            settingsToUse = mortarSettings;

            currentX = initialX;
            currentY = initialY;

            // We want to remove the audio listener so that we do not have 2 audio listeners
            if (GetComponent<AudioListener>())
            {
                Destroy(GetComponent<AudioListener>());
            }
        }
    }

    private void OnPlayerReady()
    {
        enabled = true;

        // We want to remove the audio listener so that we do not have 2 audio listeners
        if (GetComponent<AudioListener>())
        {
            Destroy(GetComponent<AudioListener>());
        }
    }

    private void OnPlayerUnready()
    {
        enabled = false;
    }
    #endregion
}

[System.Serializable]
public class CameraSettings
{
    public float distance = 10f;
    public Vector2 minMaxDistance = new Vector2(2f, 25f);

    public float heightOffset = 0f;
    public Vector2 cameraMoveSpeed;

    public float minCollisionClip = 5f;

    public bool lockCameraBehindTarget;
    public bool invert = false;
}

[System.Serializable]
public class FreeCamSettings
{
    public float standardMoveSpeed = 10f;

    public Vector2 mouseSpeed;

    public bool invert = false;
}
