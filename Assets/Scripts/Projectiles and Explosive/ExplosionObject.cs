using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionObject : MonoBehaviour
{
    [SerializeField]
    private bool loop = false;

    private ParticleSystem[] m_particles;
    private AudioSource m_audio;

	// Use this for initialization
	void Start ()
    {
        m_particles = GetComponentsInChildren<ParticleSystem>();
        m_audio = GetComponent<AudioSource>();

        foreach (ParticleSystem ps in m_particles)
        {
            ps.loop = loop;
        }

        m_audio.loop = loop;
	}
	
	// Update is called once per frame
	void Update ()
    {
        CheckForDestruction();
	}

    private void CheckForDestruction()
    {
        for (int index = 0; index < m_particles.Length; index++)
        {
            if (m_particles[index].IsAlive())
            {
                return;
            }
        }

        if (m_audio.isPlaying)
        {
            return;
        }

        Destroy(gameObject);
    }
}
