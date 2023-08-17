using System.Collections;
using System.Collections.Generic;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public class Aftershock : MonoBehaviour
{
    [SerializeField]
    private AftershockSettings aftershockSettings;

    private Rigidbody m_rigidbody;
    private Explosive _explosive;
    private AudioSource m_audio;
    private bool timeSlowed = false;
    private bool ready = false;

    private bool inLevelEvent = false;

    public void AssignSettings(AftershockSettings settings)
    {
        aftershockSettings = settings;
    }

    private void RegisterEvents()
    {
        GameEventManager.instance.onAftershockPrimed += PrimeAftershock;
        GameEventManager.instance.onGameOver += OnGameOver;
        GameEventManager.instance.onLevelEventStarted += EnteredLevelEvent;
        GameEventManager.instance.onLevelEventEnded += ExitedLevelEvent;
    }

	// Use this for initialization
	void Start ()
    {
        RegisterEvents();

        GameEventManager.instance.TriggerEvent(new AftershockEvent(GameEventType.OnAftershockUsed, aftershockSettings.neededCharge));

        m_rigidbody = GetComponent<Rigidbody>();
        m_audio = GetComponent<AudioSource>();
        _explosive = GetComponent<Explosive>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (ready)
        {
            SlowTime();

            if (timeSlowed)
            {
                if (ServiceLocator.Get<IInputProxyService>().GetButtonDown("Launch") && aftershockSettings.currentCharge >= aftershockSettings.neededCharge)
                {
                    Explode();
                }

                AftershockMovement();
            }
        }
    }

    private void SlowTime()
    {
        if (ServiceLocator.Get<IInputProxyService>().GetButtonDown("Slow Time") && !inLevelEvent)
        {
            //if (Time.timeScale == 1f)
            //{
                //Time.timeScale = aftershockSettings.timeScaleInAftershock;
                //Time.fixedDeltaTime = 0.02f * Time.timeScale;

                //TimeScaleAdjustmentEvent.Settings settings = new TimeScaleAdjustmentEvent.Settings(aftershockSettings.timeScaleInAftershock, AnimationCurve.EaseInOut(0, 0, 1, 1), 0.1f);

                //TimeScaleAdjustmentEvent @event = new TimeScaleAdjustmentEvent(GameEventType.OnStartTimeScaleAdjustment, settings);
                TimeScaleAdjustmentEvent @event = new TimeScaleAdjustmentEvent(GameEventType.OnStartTimeScaleAdjustment, aftershockSettings.timeScaleInAftershock);

                GameEventManager.instance.TriggerEvent(@event);
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnAftershockEntered));
            //}

            timeSlowed = true;
        }

        if (ServiceLocator.Get<IInputProxyService>().GetButtonUp("Slow Time") && !inLevelEvent)
        {
            //if (Time.timeScale != 1f)
            //{
                //Time.timeScale = 1f;
                //Time.fixedDeltaTime = 0.02f;
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnEndTimeScaleAdjustment));
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnAftershockExited));
            //}

            timeSlowed = false;
        }
    }

    private void Explode()
    {
        // Gernerating an offset position for the explosion to lift the bomb.
        Vector3 offsetPosition = transform.position;
        offsetPosition.y -= 1f;

        // Adding the force of the explosion to the bomb.
        //m_rigidbody.AddExplosionForce(aftershockSettings.aftershockSize * 0.1f, offsetPosition, aftershockSettings.aftershockRadius, 0f, ForceMode.Impulse);
        m_rigidbody.AddForce(Vector3.up * aftershockSettings.aftershockUpwardsForce, ForceMode.Impulse);

        // Now we shall reset the current aftershock charge
        aftershockSettings.currentCharge = 0f;

        // Increase the charge needed for the next aftershock explosion
        aftershockSettings.neededCharge *= 2f;

        // Telling the Explosive Component to TriggerExplosion()
        _explosive.TriggerExplosion();
        _explosive._explosiveSettings.hasExploded = false;

        // Call an OnAftershockUsed Event 
        GameEventManager.instance.TriggerEvent(new AftershockEvent(GameEventType.OnAftershockUsed, aftershockSettings.neededCharge));
    }

    private void AftershockMovement()
    {
        // Getting the players input.
        
        float inputX = ServiceLocator.Get<IInputProxyService>().GetAxis("Horizontal");
        float inputZ = -ServiceLocator.Get<IInputProxyService>().GetAxis("Vertical");

        // Getting the camera's transform
        Transform camTransform = Camera.main.transform;

        // Here we are getting a direction based off of the camera's current rotation
        // so that moving the bomb forwards will move the bomb towards the top of the camera
        Vector3 cameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;

        if (inputZ != 0)
        {
            m_rigidbody.AddForce(cameraForward * (-inputZ * aftershockSettings.aftershockMovementForce));
        }

        if (inputX != 0)
        {
            m_rigidbody.AddForce(cameraRight * (inputX * aftershockSettings.aftershockMovementForce));
        }
    }

    #region Event Methods
    private void PrimeAftershock()
    {
        ready = true;

        // We are registering to the OnScoreUpdated Event here so we can only gain score once aftershock is primed
        GameEventManager.instance.onScoreUpdated += OnScoreUpdated;

        // Here we are setting the explosives settings to the aftershock explosive settings
        _explosive._explosiveSettings.explosiveForce = aftershockSettings.aftershockSize;
        _explosive._explosiveSettings.explosionPrefab = aftershockSettings.aftershockExplosion;

        _explosive._explosiveSettings.hasExploded = false;
    }

    private void OnScoreUpdated(ScoreUpdateEvent eventData)
    {
        if (aftershockSettings == null)
        {
            Debug.LogError("Aftershock Settings has not been assigned to");
            return;
        }

        aftershockSettings.currentCharge += eventData.score;

        aftershockSettings.currentCharge = Mathf.Clamp(aftershockSettings.currentCharge, 0f, aftershockSettings.neededCharge);
    }

    private void EnteredLevelEvent()
    {
        inLevelEvent = true;
    }

    private void ExitedLevelEvent()
    {
        inLevelEvent = false;
    }

    private void OnGameOver()
    {
        enabled = false;
    }

    #endregion
}

[System.Serializable]
public class AftershockSettings
{
    public float aftershockSize = 100f;         // Explosive yield

    public float aftershockUpwardsForce = 150f;

    public float aftershockMovementForce = 5f;
    public float timeScaleInAftershock = 0.05f;

    public float neededCharge = 100f;
    public float currentCharge = 0f;

    public GameObject aftershockExplosion;
    public AudioClip slowDownEffect;
}
