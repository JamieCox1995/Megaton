using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImplosionLightning : MonoBehaviour
{
    public AudioClip clipToPlay;
    public GameObject soundObject;

    private ParticleSystem m_particles;
    private AudioSource m_audio;

	// Use this for initialization
	void Start ()
    {
        m_particles = GetComponent<ParticleSystem>();
        m_audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        ParticleSpawned();
	}

    public void ParticleSpawned()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[m_particles.main.maxParticles];
            
        int currentParticles = m_particles.GetParticles(particles);

        for (int i = 0; i < currentParticles; i++)
        {
            // Checking to see if the current particle has been living for the less than or equal to the time since last frame?
            if (particles[i].startLifetime - particles[i].remainingLifetime <= Time.deltaTime)
            {
                m_audio.PlayOneShot(clipToPlay, 1f);
            }
        }
    }
}
