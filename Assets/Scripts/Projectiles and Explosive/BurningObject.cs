using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningObject : ExplosiveObject
{
    public float burnTime = 10f;
    private bool burn = false;

    public Transform burnPosition;
    public GameObject burningEffect;

    public override void DealDamage(float force)
    {
        if(force > objSettings.damageThreshold && !burn)
        {
            StartCoroutine("Burn");
        }   
    }

    private IEnumerator Burn()
    {
        float timeToBurn = Random.Range(burnTime - 1f, burnTime + 1f);
        burn = true;

        GameObject effect = Instantiate(burningEffect, burnPosition.position, burnPosition.transform.rotation);
        effect.transform.parent = burnPosition;

        foreach (ParticleSystem ps in effect.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop();

            var main = ps.main;
            main.duration = timeToBurn;

            ps.Play();
        }

        yield return new WaitForSeconds(timeToBurn);

        OnExplode();
    }
}
