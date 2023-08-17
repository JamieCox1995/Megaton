using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinetic : Projectile
{
    [Header("Kinetic Projectile Settings")]
    public float startingSpeed = 500f;
    public float accelerationForce = 9.81f;

    public float startRadius = 1000f;

    public LayerMask interactableLayers;
    public LayerMask explodableLayers;

    [ColorUsage(false, true, 0f, 8f, 0.125f, 3f)]
    public Color glowingEmissiveColor;

    public float maxEntryTemperature = 3200f;
    private float startingTemperature = 300f;
    public AnimationCurve temperatureCurve;

    public Transform front;
    public ExplosiveSettings impactExplosionSettings;
    public ExplosiveSettings miniExplosionSettings;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        Spawn();
	}

    private void Spawn()
    {
        Vector3 impactLocation = FindObjectOfType<TargetingSystem>().GetImpactLocation();

        Vector3 pos = Random.onUnitSphere;

        pos.y = Mathf.Clamp(pos.y, 0.6f, 1f);

        pos *= startRadius;

        transform.position = pos + impactLocation;

        transform.forward = impactLocation - transform.position;

        m_rigidbody.useGravity = false;
        m_rigidbody.velocity = transform.forward * startingSpeed;
    }
	
	// Update is called once per frame
	private void FixedUpdate ()
    {
        // We are going to add a force to the rigidbody over time so that it accelerates.
        m_rigidbody.AddForce(accelerationForce * transform.forward, ForceMode.Acceleration);

        // As the object accelerates, we are going to lerp the emission of the object based on the velocity of the projectile
        SetHeatGlow();

        PreEmptCollision();
	}

    private void SetHeatGlow()
    {
        /*float currentVel = m_rigidbody.velocity.magnitude;
        float lerpVal = (currentVel - startingSpeed) / (maximumSpeed - startingSpeed);
        Color emissiveColor = Color.Lerp(Color.black, glowingEmissiveColor, lerpVal);*/

        Material mat = GetComponentInChildren<MeshRenderer>().material;
        //mat.SetColor("_EmissionColor", emissiveColor);

        float currentTemp = maxEntryTemperature * temperatureCurve.Evaluate(m_rigidbody.velocity.magnitude / maximumSpeed);

        mat.SetColor("_EmissionColor", Blackbody.GetRgbColour(currentTemp));
    }

    private void PreEmptCollision()
    {
        // In here we want to fire a raycast out in front of the projectile to see if there are any objects in front of it
        Ray ray = new Ray(front.position, m_rigidbody.velocity.normalized);
        RaycastHit hit;

        float distanceNextFrame = m_rigidbody.velocity.magnitude * (Time.fixedDeltaTime * 5f);

        if (Physics.Raycast(ray, out hit, distanceNextFrame, interactableLayers))
        {
            // There is an object in front of us that we want to clear out of the way. 
            // Spawn a small explosion in front of us.

            // We are first going to check to see if the hit object is a fractured chunk
            FracturedChunk chunk = hit.collider.gameObject.GetComponent<FracturedChunk>();

            if (chunk)
            {
                // The hit object has a FracturedChunk attached to it, but we are going to see if it is a indestructible
                // support chunk
                if (chunk.IsSupportChunk && chunk.IsDestructibleChunk())
                {
                    // We have met another immovable object and we want to do the big ass explosion
                    LargeExplosion();

                    return;
                }
            }

            SmallExplosion(hit.point);
            //explosive._explosiveSettings.hasExploded = false;
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (explodableLayers == (explodableLayers | (1 << collision.gameObject.layer)))
        {
            // Kill all mother fuckers.
            LargeExplosion();
        }

        m_rigidbody.useGravity = true;

        base.OnCollisionEnter(collision);
    }

    /// <summary>
    /// This is the large explosion that will be triggered when the projectile hits the ground or immovable object.
    /// </summary>
    private void LargeExplosion()
    {
        explosive._explosiveSettings = impactExplosionSettings;

        explosive.TriggerExplosion();
    }

    /// <summary>
    /// Smaller explosion used to clear the way for the projectile to hit the ground
    /// </summary>
    private void SmallExplosion(Vector3 explosionCentre)
    {
        /*Explosive explo = new Explosive(miniExplosionSettings);

        explo.Explode(explosionCentre);*/

        GameObject explosion = new GameObject("PreEmpted Explosion");
        Explosive explo = explosion.AddComponent<Explosive>();

        explosion.transform.position = explosionCentre;

        explo._explosiveSettings = miniExplosionSettings;
        explo.destroyGameObjectOnExplode = true;

        explo.TriggerExplosion();

        if (explosion != null) Destroy(explosion);
    }
}
