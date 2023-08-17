using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropaneCylinder : ExplosiveObject
{
    public float burnTime = 5f;
    public float ignitionForce = 10f;
    private bool burning = false;

    public Transform reliefValve;
    public GameObject trailEffect;

    public override void DealDamage(float force)
    {
        if (force > objSettings.damageThreshold)
        {
            if (!burning)
            {
                StartCoroutine("Thrust");
            }
        }
    }

    private IEnumerator Thrust()
    {
        burnTime = Random.Range(0.5f, burnTime);
        burning = true;
        // Spawn Thrust Effect
        GameObject effect = Instantiate(trailEffect, reliefValve.position, Quaternion.Euler(-transform.up));

        effect.GetComponent<ParticleSystem>().Play();

        effect.transform.parent = reliefValve;

        float timer = 0f;

        while(timer <= burnTime)
        {
            timer += Time.deltaTime;

            Rigidbody rb = GetComponent<Rigidbody>();

            if (rb.isKinematic)
                rb.isKinematic = false;

            rb.AddForce(-transform.up * 50f, ForceMode.Acceleration);

            yield return null;
        }

        OnExplode();
    }
}
