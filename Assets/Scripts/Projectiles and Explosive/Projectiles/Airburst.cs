using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airburst : Projectile
{
    public float armTime = 1.5f;
    public float detonationHeight = 5f;
    public LayerMask raycastLayers;

    private bool isArmed = false;

    protected override void Start()
    {
        base.Start();

        Invoke("ArmProjectile", armTime);
    }

    private void ArmProjectile()
    {
        isArmed = true;
    }

    protected override void Update()
    {
        base.Update();

        bool shouldExplode = TrackProjectileHeight();

        if (shouldExplode && isArmed && !explosive._explosiveSettings.hasExploded)
        {
            Explode();
            isArmed = false;
        }
    }

    private bool TrackProjectileHeight()
    {
        bool shouldExplode = false;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100f, raycastLayers))
        {
            if (hit.distance <= detonationHeight)
            {
                shouldExplode = true;
            }
        }

        return shouldExplode;
    }
}
