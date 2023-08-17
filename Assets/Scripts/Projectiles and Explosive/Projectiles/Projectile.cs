using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private bool explodeOnImpact = false;
    private bool explosionQueued;
    [SerializeField]
    private float explosionDelay = 1f;

    [SerializeField]
    protected float maximumSpeed = 100f;
    private bool impacted = false;

    protected Explosive explosive;

    [SerializeField]
    protected AftershockSettings aftershockSettings;
    protected Aftershock aftershock;
    protected bool aftershockPrimed = false;

    [Header("Audio Settings")]
    [SerializeField]
    protected float maxAudioPitch = 0.75f;
    protected AudioSource m_audio;

    protected Rigidbody m_rigidbody;

	// Use this for initialization
	protected virtual void Start ()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        explosive = GetComponent<Explosive>();

        aftershock = GetComponent<Aftershock>();

        m_audio = GetComponent<AudioSource>();

        GameEventManager.instance.onGamePaused += OnGamePaused;
        GameEventManager.instance.onGameUnpaused += OnGameUnPaused;


        if (aftershock != null)
        {
            aftershock.AssignSettings(aftershockSettings);
        }
	}
	
    protected virtual void OnGamePaused()
    {
        if (m_audio)
        m_audio.Pause();
    }

    protected virtual void OnGameUnPaused()
    {
        if (m_audio)
        m_audio.UnPause();
    }

    // Update is called once per frame
    protected virtual void Update ()
    {
		if (!explosive._explosiveSettings.hasExploded || !impacted)
        {
            if (m_audio) m_audio.pitch = (m_rigidbody.velocity.magnitude / maximumSpeed);

            LookAtVelocity();
        }
        else
        {
            if (m_audio && !aftershock)
            {
                m_audio.Stop();
            }
        }
	}

    private void FixedUpdate()
    {
        float speed = m_rigidbody.velocity.magnitude;

        if (speed > maximumSpeed)
        {
            //Debug.LogFormat("Overspeed! {0} > {1}", speed, maximumSpeed);

            float force = speed - maximumSpeed;

            Vector3 forceDir = -m_rigidbody.velocity / speed;

            m_rigidbody.AddForce(forceDir * force, ForceMode.VelocityChange);
        }
    }

    private void LookAtVelocity()
    {
        Vector3 lookDirection = Vector3.RotateTowards(transform.forward, m_rigidbody.velocity, 1 * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    // Use this to detect when the projectile collides with an
    // object in the world
    protected virtual void OnCollisionEnter(Collision collision)
    {
        OnImpact();
    }

    // Use this to execute logic for the impact of the projectile
    protected virtual void OnImpact()
    {
        impacted = true;

        if (aftershockPrimed == false)
        {
            if (explodeOnImpact)
            {
                if (!explosive._explosiveSettings.hasExploded)
                {
                    Explode();
                }
            }
            else
            {
                if (!explosive._explosiveSettings.hasExploded && !explosionQueued)
                {
                    StartCoroutine("QueueExplosion");
                }
            }
        }
    }

    // Use to execute the explosion
    protected virtual void Explode()
    {
        explosionQueued = true;

        explosive.TriggerExplosion();

        // Checking to see if we should destroy the bomb or not
        Aftershock aftershock = GetComponent<Aftershock>();

        if (aftershock == null)
        {
            GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnProjectileDestroyed));

            GameEventManager.instance.onGamePaused -= OnGamePaused;
            GameEventManager.instance.onGameUnpaused -= OnGameUnPaused;

            Destroy(gameObject);
        }
        else
        {
            if (aftershockPrimed == false)
            {
                aftershockPrimed = true;
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnAftershockPrimed));
            }
        }
    }

    protected IEnumerator QueueExplosion()
    {
        explosionQueued = true;

        yield return new WaitForSeconds(explosionDelay);

        Explode();
    }
}