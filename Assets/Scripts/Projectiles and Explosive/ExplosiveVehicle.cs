using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveVehicle : BurningObject
{
    [SerializeField]
    private GameObject[] wheels;

    protected override void OnExplode()
    {
        /*==================================
         * We are not calling the base method here as the vehicles will remain in the scene.
         ===================================*/ 

        // We want to unparent each wheel and add an explosive force to them
        foreach(GameObject wheel in wheels)
        {
            wheel.transform.parent = null;

            wheel.AddComponent(typeof(Rigidbody));
            wheel.GetComponent<Rigidbody>().AddExplosionForce(settings.explosiveForce, transform.position,  -0.5f);
        }

        // Calling explode on the explosive component
        explosive.TriggerExplosion();
    }

}
