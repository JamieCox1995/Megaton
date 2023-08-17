using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Homing : Projectile
{
    public float maxSpeed = 30f;
    private float currentSpeed;
    public float rotationSpeed = 2f;
    public float timeTilMaxSpeed = 5f;
    private float timeSinceStart = 0f;
    private float lerpSpeed;

    private float flightTime = 0f;

    private Vector3 targetLocation;

    public float armTime = 2f;
    private bool armed = false;

    public GameObject exhaustEffect;

    [Header("Homing Audio")]
    [SerializeField]private AudioClip lockedOnBeep;
    [SerializeField]private AudioClip thrusterSFX;
    [SerializeField]private AudioSource effectAudioSource;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        targetLocation = FindObjectOfType<TargetingSystem>().GetImpactLocation();

        Invoke("LockOn", armTime - lockedOnBeep.length);
        Invoke("ArmProjectile", armTime);
	}
	
    private void LockOn()
    {
        effectAudioSource.Stop();
        effectAudioSource.clip = lockedOnBeep;
        effectAudioSource.Play();
    }

    private void ArmProjectile()
    {
        armed = true;

        effectAudioSource.clip = thrusterSFX;
        effectAudioSource.loop = true;
        effectAudioSource.Play();

        exhaustEffect.GetComponent<ParticleSystem>().Play();

        currentSpeed = m_rigidbody.velocity.magnitude;

        lerpSpeed = currentSpeed / maxSpeed;
        timeSinceStart = timeTilMaxSpeed * lerpSpeed;
    } 

	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();

        if (armed && !explosive._explosiveSettings.hasExploded)
        {
            HeadToTarget();
        }

        flightTime += Time.deltaTime;

        if (flightTime >= 60f)
        {
            AchievementManager._instance.UnlockAchievement(AchievementType.ACH_ONE_MINUTE_FLIGHT);
        }
	}

    protected override void Explode()
    {
        base.Explode();

        effectAudioSource.Stop();
        exhaustEffect.GetComponent<ParticleSystem>().Stop();
    }

    private void HeadToTarget()
    {
        Vector3 directionTo = targetLocation - transform.position;

        float step = rotationSpeed * Time.deltaTime;
        Vector3 dir = Vector3.RotateTowards(transform.forward, directionTo, step, 0.0f);

        transform.rotation = Quaternion.LookRotation(dir);

        lerpSpeed = timeSinceStart / timeTilMaxSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, lerpSpeed);
        timeSinceStart += Time.deltaTime;

        m_rigidbody.velocity = transform.forward * currentSpeed;
    }
}
