using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveObject : PhysicsObject
{

    public ExplosiveSettings settings;
    protected Explosive explosive;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        explosive = GetComponent<Explosive>();
	}

    public override void DealDamage(float force)
    {
        base.DealDamage(force);

        if (force >= objSettings.damageThreshold)
        {
            OnExplode();
        }
    }

    public void Explode()
    {
        OnExplode();
    }

    protected virtual void OnExplode()
    {
        explosive.TriggerExplosion();

        // Destroying the gameobject so that the player can only get 1 pick up
        Destroy(gameObject);
    }
}
