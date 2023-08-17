using System.Collections;
using System.Collections.Generic;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public class Bouncing : Projectile
{
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();

		if (!explosive._explosiveSettings.hasExploded && ServiceLocator.Get<IInputProxyService>().GetButtonDown("Launch"))
        {
            Explode();

            StopCoroutine("QueueExplosion");
        }
	}
}
