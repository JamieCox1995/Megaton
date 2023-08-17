using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Explosive))]
public class OilPump : MonoBehaviour
{
    public Transform pressureVentLocation;
    public GameObject pressureVentEffect;

    private Explosive explosive;
    public ConnectedExplosive connectedExplosive;


	// Use this for initialization
	void Start ()
    {
        GameEventManager.instance.onObjectDamaged += BuildingDamaged;
        explosive = GetComponent<Explosive>();
	}

    private void BuildingDamaged(ObjectDamagedEvent objectDamagedEvent)
    {
        if (objectDamagedEvent.target != GetComponent<FracturedObject>()) return;
        
        // We want to trigger the explosion on the explosive.
        explosive.TriggerExplosion();

        // We want to tell the connected explosive to get ready to explode.
        connectedExplosive.QueueExplosion();

        // Now we want to get all of the other oilpumps and tell them to start their venting effects
        OilPump[] pumps = FindObjectsOfType<OilPump>();

        foreach(OilPump pump in pumps)
        {
            pump.VentPressure();
        }
    }

    public void VentPressure()
    {

    }
}
