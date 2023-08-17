using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singularity : Projectile
{
    [Header("Blackhole Settings")]
    public float blackholeLifeSpan = 10f;
    public float blackholeRadius = 25f;
    public float criticalRadius = 125f;
    public float minForce = 2f;
    public float maxForce = 100f;
    public LayerMask effectableObjects;

    [Header("Singularity Settings")]
    public GameObject singularityEffect;
    public GameObject eventHorizonCollapseEffect;
    public float singularityCollapseTime = 0.5f;
    private GameObject eventHorizon;
    private GameObject pullEffect;
    private Collider[] objectsToEffect;
    private float eventHorizonScale = 1f;
    private bool collapsed = false;

    [Header("Arming Settings")]
    public float detonationHeight = 10f;
    public float armTime = 0.5f;
    public LayerMask layerMask;

    [Header("Effects and Misc. Settings")]
    public bool disableGravity;

    private bool armed = false;
    private bool beingAHole = false;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        Invoke("ArmProjectile", armTime);
	}
	
    private void ArmProjectile()
    {
        armed = true;
    }

	// Update is called once per frame
	protected override void Update ()
    {
        bool shouldTrigger = TrackProjectileHeight();

        if (armed && shouldTrigger && !explosive._explosiveSettings.hasExploded) // TRIGGERED *Angry stare*
        {
            if (!m_rigidbody.isKinematic)
            {
                m_rigidbody.isKinematic = true;
            }

            if (!beingAHole)
            {

                pullEffect = Instantiate(eventHorizonCollapseEffect, transform.position, Quaternion.identity);

                // Spawning in the event horizon as a child of ourself
                eventHorizon = Instantiate(singularityEffect, transform.position, Quaternion.identity, gameObject.transform);

                // Now we start the singularity
                StartCoroutine("SingularityEvent");

                // Making sure that we don't execute any of this logic again
                beingAHole = true;
            }
        }

        base.Update();
	}

    protected override void OnCollisionEnter(Collision collision)
    {
        //base.OnCollisionEnter(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PhysicsObject>())
        {
            ConsumeObject(other.gameObject);
        }

        if (other.gameObject.GetComponent<FracturedChunk>())
        {
            FracturedChunk chunk = other.gameObject.GetComponent<FracturedChunk>();

            chunk.DetachFromObject();

            ConsumeObject(other.gameObject);
        }
    }

    // Change this to use the cube root of the new mass of the event horizon.

    private void ConsumeObject(GameObject obj)
    {
        StopCoroutine("Grow");

        Rigidbody rb = obj.GetComponent<Rigidbody>();


        if (!obj.GetComponent<FracturedChunk>())
        {
            eventHorizonScale += rb.mass / 150f;
        }
        else
        {
            eventHorizonScale += rb.mass / 10000f;
        }

        StartCoroutine("Grow");

        GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, obj.GetComponent<PhysicsObject>() ? obj.GetComponent<PhysicsObject>().GetRemainingValue() : obj.GetComponent<AddScoreOnDamage>().GetRemainingValue()));
        Destroy(obj);
    }


    private IEnumerator Grow()
    {
        float time = 0f;

        while (time <= 0.25f)
        {
            time += Time.deltaTime;

            eventHorizon.transform.localScale = Vector3.Lerp(eventHorizon.transform.localScale, Vector3.one * eventHorizonScale, time);

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator SingularityEvent()
    {
        float lifetime = 0f;
        float gravityForce = 0f;

        // Disabing the projectile's mesh collider
        GetComponentInChildren<CapsuleCollider>().enabled = false;

        objectsToEffect = Physics.OverlapSphere(transform.position, blackholeRadius, effectableObjects);

        /*if (disableGravity)
        {
            foreach(Collider col in objectsToEffect)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = false;
                }
            }
        }*/

        while ((lifetime < blackholeLifeSpan) && (blackholeRadius * eventHorizonScale < criticalRadius))
        {
            lifetime += Time.deltaTime;

            gravityForce = Mathf.Lerp(minForce * eventHorizonScale, maxForce * eventHorizonScale, lifetime / blackholeLifeSpan);

            //PullObjects(objectsToEffect, gravityForce);

            ExplosionEvent @event = new ExplosionEvent(GameEventType.OnExplosion, transform.position, -1f * gravityForce);

            GameEventManager.instance.TriggerEvent(@event);

            yield return null;
        }

        if (blackholeRadius * eventHorizonScale >= criticalRadius)
        {
            Debug.LogWarning("Blackhole Collapsed from Critical Radius");
        }
        else
        {
            Debug.LogWarning("Blackhole Collapsed from Lifetime");
        }

        Destroy(pullEffect);

        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnCancelCoroutinesRequested));

        // Now we want to collapse the eventHorizon
        StartCoroutine("Collapse");
        
        while (!collapsed)
        {
            yield return null;
        }

        explosive._explosiveSettings.explosiveForce *= (eventHorizonScale / 25f >= 1f) ? eventHorizonScale / 25f : 1f;

        // Once it has collapsed, we can finally explode and destroy ourself.
        Explode();
    }

    private IEnumerator Collapse()
    {
        float timer = 0f;

        while (timer < singularityCollapseTime)
        {
            timer += Time.deltaTime;

            eventHorizon.transform.localScale = Vector3.Lerp(eventHorizon.transform.localScale, Vector3.zero, (timer / singularityCollapseTime));

            yield return null;
        }

        // Setting collapsed to true
        collapsed = true;

        Destroy(eventHorizon);
    }

    private void PullObjects(Collider[] objects, float pullForce)
    {
        for(int index = 0; index < objects.Length; index++)
        {
            if (objects[index] != null)
            {
                Vector3 direction = transform.position - objects[index].transform.position;
                Rigidbody rb = objects[index].GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddForce(direction.normalized * (pullForce / Vector3.Distance(objects[index].transform.position, transform.position)), ForceMode.Impulse);
                }
            }
        }
    }

    private bool TrackProjectileHeight()
    {
        bool shouldExplode = false;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100f, layerMask))
        {
            if (hit.distance <= detonationHeight)
            {
                shouldExplode = true;
            }
        }

        return shouldExplode;
    }
}
