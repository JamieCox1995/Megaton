using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    [SerializeField]
    public ObjectSettings objSettings;
    private Rigidbody m_rigidbody;

    private bool isActivated = false;
    private List<Collider> currentCollidersBelow = new List<Collider>();

    public float vapourisationForce = 120f;
    private float remainingValue;

    private bool canTakeDamage = true;
    private float timeSinceDamage = 0f;
    private float damageResetTime = 1f;

	// Use this for initialization
	protected virtual void Start ()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.isKinematic = true;

        remainingValue = objSettings.startingValue;

        canTakeDamage = true;
        timeSinceDamage = 0f;
	}
	
	// Update is called once per frame
	void Update ()
    {
        ResetDamage();
	}

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!isActivated) isActivated = true;

        if (!currentCollidersBelow.Contains(collision.collider) && Vector3.Dot(Vector3.down, collision.contacts[0].normal) > 0)
        {
            currentCollidersBelow.Add(collision.collider);
        }

        if (m_rigidbody.isKinematic)
        {
            m_rigidbody.isKinematic = false;

            StartCoroutine(DisableRigidbody());
        }

        if (canTakeDamage)  //Lets not do anything if we can't take damage
        {
            //Get and check the size of the collision force
            float collisionForce = collision.impulse.magnitude;

            if (collisionForce >= objSettings.damageThreshold)
            {
                //Deal the damage to the object.
                DealDamage(collisionForce);
            }
        }
    }

    protected void OnCollisionExit(Collision collisionData)
    {
        if (currentCollidersBelow.Contains(collisionData.collider)) currentCollidersBelow.Remove(collisionData.collider);
    }

    public virtual void ReactToExplosion(Vector3 position, float distance, float radius, float size, bool applyForce = true)
    {
        if (!isActivated) isActivated = true;

        //In here we are going to react to an explosion
        //Rigidbody rigidbody = GetComponent<Rigidbody>();

        if (m_rigidbody.isKinematic)
        {
            m_rigidbody.isKinematic = false;

            StartCoroutine(DisableRigidbody());
        }

        FracturedChunk m_fracturedChunk = gameObject.GetComponent<FracturedChunk>();

        if (!m_fracturedChunk)
        { 
            m_rigidbody.AddExplosionForce(size, position, radius, 1f, ForceMode.Impulse);
        }
        else
        {
            return;
        }

        Debug.DrawLine(transform.position, position, Color.red);

        float force = size / ((distance * distance) + 1f);

        if (force < 0f) force = 0f;
 
        if (canTakeDamage)  
        {
            //force /= m_rigidbody.mass;

            if (Mathf.Abs(force) > objSettings.damageThreshold)
            {
                DealDamage(force);
            }
        }
    }

    public virtual void DealDamage(float force)
    {
        // If the force from the explosion is enough to vapourise the object first, so that we can
        // destroy the object without having to calculate anything else.
        if (force >= vapourisationForce)
        {
            GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, remainingValue));
            Destroy(gameObject);
        }

        float cost = objSettings.startingValue * (force / 1000f);

        if (remainingValue - cost <= 0f)
        {
            cost = remainingValue;

            remainingValue = 0f;
        }
        else
        {
            remainingValue -= cost;
        }

        GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, cost));
        canTakeDamage = false;
    }

    public virtual void VapouriseObject()
    {
        GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, remainingValue));
        Destroy(gameObject);
    }

    private void ResetDamage()
    {
        if (!canTakeDamage)
        {
            timeSinceDamage += Time.deltaTime;
        }

        if (timeSinceDamage >= damageResetTime)
        {
            canTakeDamage = true;
            timeSinceDamage = 0f;
        }
    }

    public float GetValue()
    {
        return objSettings.startingValue;
    }

    public float GetRemainingValue()
    {
        return remainingValue;
    }

    private IEnumerator DisableRigidbody()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        yield return new WaitForSeconds(5f);

        Vector3 newPosition = transform.position;
        Quaternion newRotation = transform.rotation;

        while (currentCollidersBelow.Count == 0 || (newPosition - position).magnitude > 0.5f || Quaternion.Angle(rotation, newRotation) > 5)
        {
            yield return new WaitForSeconds(1f);
            position = newPosition;
            rotation = newRotation;

            newPosition = transform.position;
            newRotation = transform.rotation;
        }

        m_rigidbody.isKinematic = true;
    }

    public void SetObjectSettings(float startingValue, float damageThreshold, float vapourisationForce = 200f)
    {
        objSettings = new ObjectSettings(startingValue, damageThreshold);

        this.vapourisationForce = vapourisationForce;
    }
}
