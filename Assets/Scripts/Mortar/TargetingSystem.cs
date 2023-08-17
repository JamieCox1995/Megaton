using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using TotalDistraction.CameraSystem;

public class TargetingSystem : MonoBehaviour
{
    [SerializeField]
    private TargetingSettings targetingSettings;
    [SerializeField]
    private TargetingDroneSettings droneSettings;
    [SerializeField]
    private Projector crosshairProjector;

    private Transform barrel;
    private Vector3 predictedImpactLocation = Vector3.zero;
    private float playerForce;

    private bool init = false;
      
    private float currentRotation = 0f;
    private float initialAngle = 0f;            // We use this value so that we can reset the camera to this angle on events

    private bool useAltCamera = true;

    private bool onFinalShot = false;

	// Use this for initialization
    private void Initialize()
    {
        crosshairProjector.material.color = droneSettings.crosshairColour;

        // Initializing the arrays for the trajectory predition
        targetingSettings.calculaltedPoints = new Vector3[targetingSettings.numberOfPoints];

        UpdateTrajectory();

        CalculateImpactLocation();

        GameEventManager.instance.onMortarFire += OnFire;
        GameEventManager.instance.onTurnEnd += TurnEnd;

        GameEventManager.instance.onAftershockPrimed += OnFinalShot;

        TurnEnd();

        init = true;
    }

    // Update is called once per frame
    void Update ()
    {
        if (!init && barrel != null)
        {
            Initialize();
        }
        else
        {
            UpdateTrajectory();
            CalculateImpactLocation();
            UpdateTargetDrone();
        }
	}

    /*=============================================================
     * CHANGE ME:  
     * Targeting Drone object rotates to match the angle of the mortar in
     * the non-free look mode.
     * We rotate the Camera object for free look mode.
     * Apply height update to the camera rather than the entire drone.
     *===========================================================*/
    public void RotateProjector(float angle)
    {
        Quaternion rotated = Quaternion.Euler(90f, angle, 0f);

        crosshairProjector.gameObject.transform.rotation = rotated;
        droneSettings.lookAtTarget.rotation = Quaternion.Euler(0, angle, 0);

        if ((useAltCamera && !ServiceLocator.Get<IInputProxyService>().GetButton("FreeLook")) || (!useAltCamera && ServiceLocator.Get<IInputProxyService>().GetAxis("Target Camera Rotation") == 0))
        {

            currentRotation = angle;
        }
    }

    // Use this to calculate the trajectory of the projectile from the 
    // current elevation and rotation of the mortar
    private void UpdateTrajectory()
    {
        targetingSettings.calculaltedPoints = new Vector3[targetingSettings.numberOfPoints + 1];

        Vector3 velocity = barrel.transform.forward * playerForce;

        for(int index = 0; index < targetingSettings.numberOfPoints; index++)
        {
            float time = index * targetingSettings.timeBetweenPoints;

            targetingSettings.calculaltedPoints[index] = new Vector3(velocity.x * time, (velocity.y * time) - (.5f * 9.81f  * Mathf.Pow(time, 2f)), velocity.z * time) + barrel.position;

        }
    }

    // Use this to calculate the impact location of the projectile
    private void CalculateImpactLocation()
    {
        for (int index = 0; index < targetingSettings.calculaltedPoints.Length - 1; index++)
        {
            Vector3 direction = targetingSettings.calculaltedPoints[index + 1] - targetingSettings.calculaltedPoints[index];
            Ray ray = new Ray(targetingSettings.calculaltedPoints[index], direction);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, direction.magnitude, targetingSettings.layerMask))
            {
                predictedImpactLocation = hit.point;
                droneSettings.lookAtTarget.position = predictedImpactLocation;

                break;
            }
        }
    }

    private void UpdateTargetDrone()
    {
        Vector3 droneLocation = new Vector3(predictedImpactLocation.x, predictedImpactLocation.y, predictedImpactLocation.z);

        droneSettings.droneObject.transform.position = Vector3.Lerp(droneSettings.droneObject.transform.position, droneLocation, Time.deltaTime * droneSettings.movementSpeed);
    }

    private void OnFire(MortarFireEvent onFire)
    {
        crosshairProjector.enabled = false;
    }

    private void TurnEnd()
    {
        crosshairProjector.enabled = true;

        currentRotation = initialAngle;

        CameraSystem.Target = droneSettings.droneObject;
    }

    public void SetPositionAndForce(Transform barrelTransform, float force, float initialAngle)
    {
        this.initialAngle = initialAngle;
        barrel = barrelTransform;
        playerForce = force;
    }

    public Vector3 GetImpactLocation()
    {
        return predictedImpactLocation;
    }

    public GameObject GetLookAtTarget()
    {
        return droneSettings.lookAtTarget.gameObject;
    }

    private void OnFinalShot()
    {
        onFinalShot = true;
    }
}

[System.Serializable]
public class TargetingSettings
{
    public int numberOfPoints = 200;
    public int numberToDraw = 10;
    public float timeBetweenPoints = 1f;
    public GameObject markerPrefab;
    public LayerMask layerMask;
    [HideInInspector]
    public GameObject[] spawned;
    [HideInInspector]
    public Vector3[] calculaltedPoints;
}

[System.Serializable]
public class TargetingDroneSettings
{
    public Vector2 cameraHeightLimits;
    public float heightSensitivity = 3f;
    public float movementSpeed = 5f;

    public Color crosshairColour;

    public GameObject droneObject;
    public Transform lookAtTarget;
}
