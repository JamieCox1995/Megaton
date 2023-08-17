using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;

public class Acid : Projectile
{
    [Header("Acid Effects")]
    public GameObject sprayEffect;
    public GameObject explosionSpray;

    public AudioClip releaseSFX;
    public GameObject releaseSFXSource;
    public Vector2 _3dAudioSettings = new Vector2(15f, 300f);

    private bool acidSprayed = false;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();

        if (ServiceLocator.Get<IInputProxyService>().GetButtonDown("Launch") && !acidSprayed)
        {
            SprayAcid();
        }
	}

    protected override void Explode()
    {
        //base.Explode();

        if (!acidSprayed)
        {
            acidSprayed = true;

            // Here we just want to spawn in the explosionSpray and call on Projectile Destroyed
            Instantiate(explosionSpray, transform.position, Quaternion.identity);

            PlayAudio();

            if (!aftershock)
            {
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnProjectileDestroyed));

                GetComponent<TrailRenderer>().enabled = false;
                m_audio.enabled = false;

                Destroy(this);
            }
            else
            {
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnAftershockPrimed));
            }
        }
    }

    private void SprayAcid()
    {
        acidSprayed = true;

        // Here we just want to spawn in the sprayEffect and call on Projectile Destroyed
        Instantiate(sprayEffect, transform.position, transform.rotation);

        PlayAudio();

        if (!aftershock)
        {
            GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnProjectileDestroyed));

            Destroy(gameObject);
        }
        else
        {
            GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnAftershockPrimed));
        }
    }

    private void PlayAudio()
    {
        // Playing some hissing audio
        GameObject soundObject = Instantiate(releaseSFXSource, transform.position, Quaternion.identity);
        AudioSource source = soundObject.GetComponent<AudioSource>();

        source.minDistance = _3dAudioSettings.x;
        source.maxDistance = _3dAudioSettings.y;

        source.PlayOneShot(releaseSFX, 1f);

        Destroy(soundObject, releaseSFX.length);
    }
}
