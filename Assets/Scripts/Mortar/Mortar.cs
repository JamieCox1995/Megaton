using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using TotalDistraction.CameraSystem;

public class Mortar : MonoBehaviour
{
    [SerializeField]
    private MortarSettings mortarSettings;

    [SerializeField]
    private TargetingSystem targetingSystem;
    [SerializeField]
    private AmmunitionLoader ammunitionLoader;

    [Header("Starting Values: ")]
    public float startingElevation = 0f;
    public float startingRotation = 0f;

    [Header("GUI")]
    [SerializeField]
    private Text  currentForce;

    [SerializeField]
    private Text currentElev;

    [SerializeField]
    private RectTransform barrelUI, forceUI;


    private AudioSource m_audio;
    [Header("Audio Settings")]
    [SerializeField]private AudioClip movementStartSound;
    [SerializeField]private AudioClip movementLoopSound;
    [SerializeField]private AudioClip movementEndSound;
    private bool loopPlayedLast = false;
    private bool startPlayed = false;


    private float currentElevation, currentRotation;

    [SerializeField]
    private bool playerReady = false;   // Used to stop the player from accidentally firing the first projectile

    private bool paused = false;


    private void OnValidate()
    {
        currentElevation = startingElevation;
        currentRotation = startingRotation;

        Quaternion newRotation = Quaternion.Euler(-currentElevation, currentRotation, 0f);
        mortarSettings.mortarGameObject.transform.localRotation = newRotation;
    }

    // Use this for initialization
    private void Initialize()
    {
        RegisterEvents();

        m_audio = GetComponent<AudioSource>();

        GetAmmunitionFromPrefs();

        targetingSystem.SetPositionAndForce(mortarSettings.mortarBarrel, mortarSettings.currentForce, transform.localEulerAngles.y);

        playerReady = false;    // Setting the player's readiness to false so that they must press a button before continuing

        mortarSettings.projectile = ammunitionLoader.RetrieveAmmunitionPrefab(mortarSettings.ammunitionType);

        //currentElevation = mortarSettings.startingElevation;

        UpdateGUI();
    }

    private void RegisterEvents()
    {
        GameEventManager.instance.onPlayerReady += OnPlayerReady;
        GameEventManager.instance.onPlayerUnready += OnPlayerUnready;

        GameEventManager.instance.onGamePaused += OnPaused;
        GameEventManager.instance.onGameUnpaused += OnUnpaused;

        GameEventManager.instance.onPickupGained += GainPickup;

        GameEventManager.instance.onTurnEnd += OnEndTurn;
    }

	void Start ()
    {
        Initialize();	
	}

    private void GetAmmunitionFromPrefs()
    {
        mortarSettings.ammunitionType = PlayerProgression._instance.GetEquippedProjectile(ProjectileType.Primary);
        mortarSettings.primaryAmmunition = ammunitionLoader.RetrieveAmmunitionPrefab(mortarSettings.ammunitionType);
    }

    // Update is called once per frame
    void Update ()
    {
        targetingSystem.SetPositionAndForce(mortarSettings.mortarBarrel, mortarSettings.currentForce, transform.eulerAngles.y);

        if (playerReady && !paused)
        {
            HandleInput();
            UpdateGUI();
            Audio();
        }
	}

    private void UpdateGUI()
    {

        currentForce.text = "Current Force: " + mortarSettings.currentForce.ToString("N0");
        currentElev.text = "Current Elevation:" + currentElevation.ToString("N0");

        float scale = (mortarSettings.currentForce - mortarSettings.launchForce.x) / (mortarSettings.launchForce.y - mortarSettings.launchForce.x);

        forceUI.localScale = Vector3.one * scale;

        barrelUI.eulerAngles = new Vector3(0f, 0f, -currentElevation);
    }

    // Use this to handle the player's input
    private void HandleInput()
    {
        ElevationRoation();

        LaunchForce();

        Fire();
    }

    private void ElevationRoation()
    {
        float elevation = ServiceLocator.Get<IInputProxyService>().GetAxis("Vertical");
        currentElevation += elevation * mortarSettings.elevationChangeRate * Time.deltaTime;

        currentElevation = Utilities.ValidateAngle(currentElevation);
        currentElevation = Mathf.Clamp(currentElevation, mortarSettings.elevationClamp.x, mortarSettings.elevationClamp.y);

        float rotation = ServiceLocator.Get<IInputProxyService>().GetAxis("Horizontal");
        currentRotation += rotation * mortarSettings.rotationRate * Time.deltaTime;

        Quaternion newRotation = Quaternion.Euler(-currentElevation, currentRotation, 0f);
        mortarSettings.mortarGameObject.transform.localRotation = newRotation;
        targetingSystem.RotateProjector(transform.localEulerAngles.y + currentRotation);
    }

    private void LaunchForce()
    {
        float force = ServiceLocator.Get<IInputProxyService>().GetAxis("Modify Force");

        mortarSettings.currentForce += force * mortarSettings.forceIncrement * Time.deltaTime;
        mortarSettings.currentForce = Mathf.Clamp(mortarSettings.currentForce, mortarSettings.launchForce.x, mortarSettings.launchForce.y);
    }

    // Use this to spawn in the projectile when the player fires. Also used to
    // decrement the ammunition
    private void Fire()
    {
        if (ServiceLocator.Get<IInputProxyService>().GetButtonDown("Launch") && mortarSettings.numberOfShots > 0)
        {
            LaunchProjectile();

            m_audio.Stop();
        }
    }

    private GameObject SpawnProjectile()
    {
        GameObject projectile = Instantiate(mortarSettings.projectile, mortarSettings.mortarBarrel.position, mortarSettings.mortarBarrel.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        rb.velocity = mortarSettings.mortarBarrel.transform.forward * mortarSettings.currentForce;

        return projectile;
    }

    private void GainPickup(PickupEvent pickupEvent)
    {
        // In here we want to change our active ammo to the one which was granted in the pickup.
        mortarSettings.hasPickup = true;

        // Getting the prefab for the pickup ammunition
        GameObject projectile = ammunitionLoader.RetrieveAmmunitionPrefab(pickupEvent.pickupType);

        // Setting our active ammumition to the projectile prefab
        mortarSettings.projectile = projectile;
    }

    private void Audio()
    {
        // Here we want to play the hydraulic loop if Vertical or Horizontal is being pressed,
        if (ServiceLocator.Get<IInputProxyService>().GetAxis("Vertical") != 0 || ServiceLocator.Get<IInputProxyService>().GetAxis("Horizontal") != 0)
        {
            if (!startPlayed)
            {
                m_audio.clip = movementStartSound;
                m_audio.Play();
                startPlayed = true;
            }
            else
            {
                if (!loopPlayedLast)
                {
                    m_audio.Stop();
                    m_audio.loop = true;
                    m_audio.clip = movementLoopSound;
                    m_audio.Play();
                }

                loopPlayedLast = true;
            }

        }
        else
        {
            // Here we are playing the end sound if the mortar has stopped moving
            if (ServiceLocator.Get<IInputProxyService>().GetAxisRaw("Vertical") == 0 && ServiceLocator.Get<IInputProxyService>().GetAxisRaw("Horizontal") == 0)
            {
                if (loopPlayedLast)
                {
                    loopPlayedLast = startPlayed = false;

                    m_audio.loop = false;
                    m_audio.Stop();
                    m_audio.clip = movementEndSound;
                    m_audio.Play();
                }
            }
        }
    }

    private void OnPlayerReady()
    {
        if (!paused)
            playerReady = true;
    }

    private void OnPlayerUnready()
    {
        if (!paused)
            playerReady = false;
    }

    private void OnPaused()
    {
        m_audio.Stop();
        paused = true;
    }

    private void OnUnpaused()
    {
        paused = false;
    }

    private void OnEndTurn()
    {
        playerReady = true;
    }

    
    public void SetElevation(float elevation)
    {
        currentElevation = elevation;
    }

    public float GetElevation()
    {
        return currentElevation;
    }

    public void SetRotation(float rotation)
    {
        currentRotation = rotation;
    }

    public float GetRotation()
    {
        return currentRotation;
    }

    public void SetForce(float force)
    {
        mortarSettings.currentForce = force;
    }

    public float GetForce()
    {
        return mortarSettings.currentForce;
    }

    public void LaunchProjectile()
    {
        // Spawn a projectile
        GameObject proj = SpawnProjectile();

        if (mortarSettings.numberOfShots == 1)
        {
            if (!mortarSettings.hasPickup)
            {
                proj.AddComponent(typeof(Aftershock));
            }
        }

        if (mortarSettings.hasPickup)
        {
            mortarSettings.projectile = mortarSettings.primaryAmmunition;
            mortarSettings.hasPickup = false;
            //GameEventManager.instance.TriggerEvent(new SwitchAmmoEvent(GameEventType.OnAmmoChanged, 0));
        }
        else
        {
            mortarSettings.numberOfShots--;
        }

        // Spawn the firing effect, play a sound and an animation
        Instantiate(mortarSettings.muzzleEffect, mortarSettings.mortarBarrel.position, mortarSettings.mortarBarrel.rotation);

        CameraSystem.Target = proj;

        GameEventManager.instance.TriggerEvent(new MortarFireEvent(GameEventType.OnMortarFired, proj, mortarSettings.numberOfShots));

        playerReady = false;
    }

    public int GetNumberOfShots()
    {
        return mortarSettings.numberOfShots;
    }

    public void SetNumberOfShots(int n)
    {
        if (n <= 0) throw new System.ArgumentOutOfRangeException("n");

        mortarSettings.numberOfShots = n;
    }
}

[System.Serializable]
public class MortarSettings
{
    [Header("Mortar Key Objects")]
    public GameObject mortarGameObject;                         // This is the GameObject of the in-game barrel which will be rotated and elevated
    public Transform mortarBarrel;                              // The position which the effects and projectiles will be spawned from when firing

    [Space]
    public GameObject projectile;                               // Holds the prefab of the currently selected projectile
    public GameObject muzzleEffect;                             // Prefab for the effect when a projectile is launched

    [Header("Elevation and Rotation Settings")]
    public float startingElevation = 15f;
    public Vector2 elevationClamp = new Vector2(-10f, 70f);     // Used to clamp the elevation of the mortar
    public float elevationChangeRate = 6f;                      // Rate at which the mortar's elevation changes
    public float rotationRate = 5f;                             // Rate at which the mortar's rotation changes

    [Header("Projectile Settings")]
    public int numberOfShots = 3;                               // Numboer of shots the player has to complete the level

    public string ammunitionType;                             // Holds the player's selected ammunitions
    public GameObject primaryAmmunition;

    [HideInInspector]
    public GameObject activeProjectile;
    public bool hasPickup = false;

    [Header("Projectile Launch Settings")]
    public float currentForce = 60f;                            // Current force at which the projectile will be launch at
    public Vector2 launchForce = new Vector2(20f, 85f);         // Minimum and Maximum force for a projectile launch
    public float forceIncrement = 10f;                          // Rate at which the launch force changes
}
