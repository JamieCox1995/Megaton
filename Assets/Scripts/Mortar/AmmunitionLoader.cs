using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmunitionLoader : MonoBehaviour
{
    // Standard, Cluster, Airburst, Homing, Bouncing, Gravity, Singularity, Nuke

    [Header("Primary Ammunitions")]
    [SerializeField]
    private GameObject standardAmmunition;
    [SerializeField]
    private GameObject acidAmmunition;
    [SerializeField]
    private GameObject airburstAmmunition;
    [SerializeField]
    private GameObject bouncingAmmunition;
    [SerializeField]
    private GameObject implosionAmmunition;
    [SerializeField]
    private GameObject clusterAmmunition;

    [Header("Secondary Ammunitions")]
    [SerializeField]
    private GameObject homingAmmunition;
    [SerializeField]
    private GameObject laserAmmunition;
    [SerializeField]
    private GameObject singularityAmmunition;
    [SerializeField]
    private GameObject gravityAmmunition;
    [SerializeField]
    private GameObject nukeAmmunition;
    [SerializeField]
    private GameObject kineticAmmunition;

    // Used to retrieve the correct prefab for the current ammunition
	public GameObject RetrieveAmmunitionPrefab(string ammunition)
    {
        GameObject toReturn = standardAmmunition;

        switch (ammunition)
        {
            case "Airburst":
                toReturn = airburstAmmunition;
                break;
            case "Bouncing":
                toReturn = bouncingAmmunition;
                break;
            case "Cluster":
                toReturn = clusterAmmunition;
                break;

            case "Homing":
                toReturn = homingAmmunition;
                break;
            case "Singularity":
                toReturn = singularityAmmunition;
                break;
            case "Gravity":
                toReturn = gravityAmmunition;
                break;
            case "Nuke":
                toReturn = nukeAmmunition;
                break;
            case "Acid":
                toReturn = acidAmmunition;
                break;
            case "Implosion":
                toReturn = implosionAmmunition;
                break;
            case "Laser":
                toReturn = laserAmmunition;
                break;
            case "Kinetic":
                toReturn = kineticAmmunition;
                break;
        }

        return toReturn;
    }
}
