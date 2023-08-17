using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImplosionEffect : MonoBehaviour
{
    [SerializeField]
    private float buildupTime = 1f;
    [SerializeField]
    private float explosionDelay = 1.3f;

    [SerializeField]
    private ExplosiveSettings explosiveSettings;
    private ExplosiveSettings implosionSettings;
    private Explosive explosive;

    private bool aftershock = false;
    private AudioSource m_audio;

    private void Start()
    {
        m_audio = GetComponent<AudioSource>();
        explosive = GetComponent<Explosive>();

        GameEventManager.instance.onGamePaused += OnPause;
        GameEventManager.instance.onGameUnpaused += OnUnPause;
    }

    public void StartImplosion(ExplosiveSettings implo, bool lastShot)
    {
        implosionSettings = implo;

        aftershock = lastShot;

        Invoke("Implode", buildupTime + 0.1f);
        Invoke("Explode", explosionDelay + buildupTime);
    }

    private void Implode()
    {
        if (m_audio.isPlaying)
        {
            m_audio.Stop();
        }

        GameEventManager.instance.TriggerEvent(new ExplosionEvent(GameEventType.OnExplosion, transform.position, implosionSettings.explosiveForce));
    }

    private void Explode()
    {
        explosive.TriggerExplosion();

        if (!aftershock)
        {
            GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnProjectileDestroyed));
        }
        else
        {
            GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnAftershockPrimed));
        }


        GameEventManager.instance.onGamePaused -= OnPause;
        GameEventManager.instance.onGameUnpaused -= OnUnPause;
    }

    private void OnPause()
    {
        m_audio.Pause();
    }

    private void OnUnPause()
    {
        m_audio.UnPause();
    }
}
