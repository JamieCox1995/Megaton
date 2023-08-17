using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : Projectile
{
    public float effectedRadius = 30f;
    public float force = 20f;
    public float duration = 3.5f;
    private bool gravity = false;

    private Collider[] effecting;

    public GameObject effectPrefab;
    public LayerMask objsToEffect;

    protected override void OnImpact()
    {
        if (!gravity)
        {
            StartCoroutine("StartEffect");
        }
    }

    private IEnumerator StartEffect()
    {
        float timer = 0f;

        effecting = Physics.OverlapSphere(transform.position, effectedRadius, objsToEffect);

        m_rigidbody.isKinematic = true;

        while (timer < duration)
        {
            ReverseGravity();

            timer += Time.deltaTime;
            yield return null;
        }

        m_rigidbody.isKinematic = true;

        Explode();
    }

    private void ReverseGravity()
    {
        for (int i = 0; i < effecting.Length; i++)
        {
            if (effecting[i] != null && (effecting[i].gameObject.GetComponent<PhysicsObject>() || effecting[i].gameObject.GetComponent<FracturedChunk>()))
            {

                FracturedChunk chunk = effecting[i].gameObject.GetComponent<FracturedChunk>();

                if (chunk != null)
                {
                    if (chunk.IsSupportChunk) continue;
                }


                Rigidbody rb = effecting[i].GetComponent<Rigidbody>();

                rb.isKinematic = false;

                float dist = Vector3.Distance(transform.position, effecting[i].transform.position);
                float gf = -Physics.gravity.y * force;

                gf *= (1 - (dist / effectedRadius));

                if (dist <= effectedRadius)
                {
                    rb.AddForce(Vector3.up * gf, ForceMode.Acceleration);
                }
                else
                {
                    effecting[i] = null;
                }
            }
        }
    }

}
