using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ScoredObject), typeof(Rigidbody))]
public class PhysObject : MonoBehaviour
{
    [Header("Force Threshold Values:")]
    public float damageThreshold = 250f;
    public float vapourisationForce = 1000f;

    [Header("Rigidbody Settings:")]
    public bool disableRigidbodyOnStart = true;
    public bool disableRigidbodyAtRest = false;

    [Header("Events:")]
    public UnityEvent onObjectDamaged;

    private ScoredObject _scoredObject;
    private Rigidbody _rigidbody;

    private bool canTakeDamage = false;
    private float sinceLastDamage = 0f;
    private float damageRestTime = 0.2f;

    protected virtual void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = disableRigidbodyOnStart;

        _scoredObject = GetComponent<ScoredObject>();

        canTakeDamage = true;
        sinceLastDamage = 0f;
    }

    private void Update()
    {
        ResetDamage();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        EnableRigidbody();

        if (canTakeDamage == true)
        {
            float forceOfCollision = collision.impulse.magnitude;

            if (forceOfCollision >= vapourisationForce)
            {
                // The force of the collision between us and another object has caused us to be vapourised
                _scoredObject.AwardAllValue();

                onObjectDamaged.Invoke();

                Destroy(gameObject);
            }
            else
            {
                if (forceOfCollision >= damageThreshold)
                {
                    _scoredObject.AwardValue(forceOfCollision);
                    onObjectDamaged.Invoke();

                    canTakeDamage = false;
                }
            }
        }
    }

    public void OnExplosion(Vector3 position, float distance, float radius, float size, bool applyForce = true)
    {
        // We are checking to see if there is a FracturedChunk applied to this object so that force is only applied to it once.
        if (GetComponent<FracturedChunk>()) return;

        EnableRigidbody();

        float explosionForce = size / ((distance * distance) + 1f);

        if (explosionForce <= 0f) return;

        _rigidbody.AddExplosionForce(size, position, radius, 1f, ForceMode.Impulse);

        onObjectDamaged.Invoke();

        if (canTakeDamage == true)
        {
            if (Mathf.Abs(explosionForce) >= damageThreshold)
            {
                _scoredObject.AwardValue(explosionForce);
            }
        }
    } 

    private void EnableRigidbody()
    {
        if (_rigidbody.isKinematic == true) _rigidbody.isKinematic = false;
    }

    private void ResetDamage()
    {
        if (canTakeDamage == true) return;

        sinceLastDamage += Time.deltaTime;

        if (sinceLastDamage >= damageRestTime)
        {
            canTakeDamage = true;
            sinceLastDamage = 0f;
        }
    }
}
