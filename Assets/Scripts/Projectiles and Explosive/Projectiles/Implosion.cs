using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;

public class Implosion : Projectile
{
    [SerializeField] private float armTime = 0.2f;
    [SerializeField] private float detonationHeight = 10f;
    [SerializeField] private LayerMask raycastLayers;
    private bool isArmed = false;
    private bool exploded = false;

    private bool gamePaused = false;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        Invoke("Arm", armTime);
	}

    protected override void OnGamePaused()
    {
        gamePaused = true;
    }

    protected override  void OnGameUnPaused()
    {
        gamePaused = false;
    }

    private void Arm()
    {
        isArmed = true;
    }
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();

        bool shouldExplode = ((TrackProjectileHeight() || ServiceLocator.Get<IInputProxyService>().GetButtonDown("Launch")) && gamePaused == false);

        if (shouldExplode && isArmed && !exploded)
        {
            isArmed = false;
            Explode();
        }
	}

    protected override void OnCollisionEnter(Collision collision)
    {
        if (isArmed) base.OnCollisionEnter(collision);
    }

    protected override void Explode()
    {
        GameObject implosionEffect = Instantiate(explosive._explosiveSettings.explosionPrefab, transform.position, Quaternion.identity);

        implosionEffect.GetComponent<ImplosionEffect>().StartImplosion(explosive._explosiveSettings, (aftershock != null) ? true : false);

        exploded = true;

        if (!aftershock)
        {
            Destroy(gameObject);
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
