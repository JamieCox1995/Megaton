using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;

public class Cluster : Projectile
{
    [SerializeField]
    private GameObject childProjectile;
    [SerializeField]
    private int numberOfChildrenToDeploy = 40;

    private bool deployed = false;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();

        if (ServiceLocator.Get<IInputProxyService>().GetButtonDown("Launch") && !deployed && !explosive._explosiveSettings.hasExploded)
        {
            DeployChildrenBombs();
        }
	}

    private void DeployChildrenBombs()
    {
        for (int i = 0; i < numberOfChildrenToDeploy; i++)
        {
            Vector3 ranPos = Random.onUnitSphere + transform.position;
            Vector3 directionToCentre = transform.position - ranPos;

            GameObject child = Instantiate(childProjectile, ranPos, transform.rotation);

            Rigidbody rb = child.GetComponent<Rigidbody>();

            rb.velocity = m_rigidbody.velocity * Random.Range(0.5f, 1.5f);

            rb.AddForce(directionToCentre * -50f, ForceMode.Impulse);
        }

        deployed = true;
    }
}
