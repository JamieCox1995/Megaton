using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Explosive))]
public class ConnectedExplosive : MonoBehaviour
{
    [Header("Explosive Properites: ")]
    public Explosive explosive;
    public float nextExplosionDelay = 0.5f;
    public ConnectedExplosive nextExplosive;

    public bool test = false;

	// Use this for initialization
	void Start ()
    {
        explosive = GetComponent<Explosive>();   	

        if (test)
        {
            Explode();
        }
	}

    public void QueueExplosion()
    {
        Invoke("Explode", nextExplosionDelay);   
    }

    private void Explode()
    {
        if (nextExplosive != null)
        {
            nextExplosive.QueueExplosion();
        }

        explosive.TriggerExplosion();
    }
}
