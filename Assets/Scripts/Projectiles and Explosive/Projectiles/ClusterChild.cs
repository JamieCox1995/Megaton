using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterChild : Projectile
{
    protected override void Explode()
    {
        explosive.TriggerExplosion();

        Destroy(gameObject);
    }
}
