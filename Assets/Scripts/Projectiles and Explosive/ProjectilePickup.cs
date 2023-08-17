using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// We inherit from PhysObject so that we can easily react to explosions
// and collisions with other objects without having to repeat code. We also require an
// Explosive Component for the explosion when a pickup is earned
[RequireComponent(typeof(Explosive))]
public class ProjectilePickup : PhysObject
{
    [SerializeField]
    private ProjectilePickupVisual homingMesh;
    [SerializeField]
    private ProjectilePickupVisual laserMesh;
    [SerializeField]
    private ProjectilePickupVisual gravityMesh;
    [SerializeField]
    private ProjectilePickupVisual singularityMesh;
    [SerializeField]
    private ProjectilePickupVisual nukeMesh;
    [SerializeField]
    private ProjectilePickupVisual kineticMesh;

    private string pickupToGive;
    private bool completed = false;

    private MeshFilter meshRenderer;
    private Explosive _explosive;

    protected override void Start()
    {
        base.Start();

        // We are registering to the OnMortarFired event so that we can destroy ourself
        GameEventManager.instance.onMortarFire += CheckForLastShot;

        // Retrieving the string for the projectile pick up from the PlayerPrefs.
        pickupToGive = PlayerProgression._instance.GetEquippedProjectile(ProjectileType.Pickup);

        meshRenderer = GetComponent<MeshFilter>();
        _explosive = GetComponent<Explosive>();

        switch(pickupToGive)
        {
            case "Homing":
                meshRenderer.mesh = homingMesh.crateMesh;
                GetComponent<MeshRenderer>().material.SetTexture("_MainTex", homingMesh.crateMaterial);
                break;
            case "Gravity":
                meshRenderer.mesh = gravityMesh.crateMesh;
                GetComponent<MeshRenderer>().material.SetTexture("_MainTex", gravityMesh.crateMaterial);
                break;
            case "Singularity":
                meshRenderer.mesh = singularityMesh.crateMesh;
                GetComponent<MeshRenderer>().material.SetTexture("_MainTex", singularityMesh.crateMaterial);
                break;
            case "Nuke":
                meshRenderer.mesh = nukeMesh.crateMesh;
                GetComponent<MeshRenderer>().material.SetTexture("_MainTex", nukeMesh.crateMaterial);
                break;
            case "Laser":
                meshRenderer.mesh = laserMesh.crateMesh;
                GetComponent<MeshRenderer>().material.SetTexture("_MainTex", laserMesh.crateMaterial);
                break;
            case "Kinetic":
                meshRenderer.mesh = kineticMesh.crateMesh;
                GetComponent<MeshRenderer>().material.SetTexture("_MainTex", kineticMesh.crateMaterial);
                break;
        }

        GetComponent<MeshCollider>().sharedMesh = meshRenderer.mesh;
    }

    public void AwardPickup()
    {
        if (completed == false)
        {
            completed = true;

            GameEventManager.instance.TriggerEvent(new PickupEvent(GameEventType.OnPickupGained, pickupToGive));

            GameEventManager.instance.onMortarFire -= CheckForLastShot;

            _explosive.TriggerExplosion();
        }
    }

    /*public override void DealDamage(float force)
    {
        if (!completed)
        {
            GetComponent<Rigidbody>().isKinematic = false;

            completed = true;

            // This is where we are going to check to see if the object should give
            // the player a special weapon
            if (force >= objSettings.damageThreshold)
            {
                // Trigger a game event that has string for the granted powerup.
                GameEventManager.instance.TriggerEvent(new PickupEvent(GameEventType.OnPickupGained, pickupToGive));

                OnExplode();
            }
        }
    }

    public override void VapouriseObject()
    {
        DealDamage(Mathf.Infinity);

        base.VapouriseObject();
    }

    protected override void OnExplode()
    {
        // Adding to the players score when they successfully acquire a pick up
        //GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, objSettings.startingValue));

        explosive.TriggerExplosion();

        GameEventManager.instance.onMortarFire -= CheckForLastShot;

        // Destroying the gameobject so that the player can only get 1 pick up
        Destroy(gameObject);
    }*/

    private void CheckForLastShot(MortarFireEvent fireEvent)
    {
        if (fireEvent.remainingShots == 0)
        {
            Debug.LogWarning("Pickup Crate has been destroyed on the players last shot.");

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}

[System.Serializable]
public class ProjectilePickupVisual
{
    public Mesh crateMesh;
    public Texture crateMaterial;
}
