using UnityEngine;
using System.Collections;

public class ParticleObject : MonoBehaviour {

    [SerializeField]
    private bool loop = false;

    private ParticleSystem[] particles;

    private void Start()
    {
        particles = GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particles)
        {
            ps.loop = loop;
        }
    }

    private void Update()
    {
        CheckForDestruction();
    }

    private void CheckForDestruction()
    {
        for (int index = 0; index < particles.Length; index++)
        {
            if (particles[index].IsAlive())
            {
                return;
            }
        }

        Destroy(gameObject);
    }
}
