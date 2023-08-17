using System.Collections;
using System.Collections.Generic;
using TotalDistraction.CameraSystem;
using UnityEngine;

public class LaserProjectile : Projectile
{
    public GameObject laserEmitterPrefab;
    public GameObject staticEmitter;

    [SerializeField]
    private float activationHeight = 5f;
    [SerializeField]
    private float armTime = 1f;
    private bool armed = false;
    private bool exploded = false;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        Invoke("Arm", armTime);
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        if (!exploded) base.Update();

        // For this projectile, we want to track the height from the ground
        bool deploy = TrackHeight();
        // When we are X units above the ground, we want to deploy the laser emitter
        if (deploy && !exploded && armed)
        {
            Deploy();
        }
	}

    private void Arm()
    {
        armed = true;
    }

    private void Deploy()
    {
        // We want to spawn in the prefab for the laser emitter, and set it's position AND
        // rotation to the same as the projectile
        GameObject prefab = Instantiate(laserEmitterPrefab, staticEmitter.transform.position, transform.rotation);

        // Here we are telling the gameplay camera to look at the laser emitter
        CameraSystem.Target = prefab;

        // We then want to destory the mesh representing emitter, and keep the fins.
        GetComponent<TrailRenderer>().enabled = false;
        GetComponent<AudioSource>().enabled = false;

        Destroy(staticEmitter);

        exploded = true;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        //base.OnCollisionEnter(collision);
    }

    private bool TrackHeight()
    {
        Ray ray = new Ray(transform.position, -Vector3.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, activationHeight))
        {
            return true;
        }

        return false;
    }
}
