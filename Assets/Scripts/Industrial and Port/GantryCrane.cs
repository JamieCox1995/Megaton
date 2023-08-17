using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GantryCrane : MonoBehaviour
{
    private GantryState currentState = GantryState.Idle;

    [Header("Gantry Steering Settings")]
    public float maxAcceleration = 0.005f;
    public float gantryMaxSpeed = 0.08f;
    public float gantrySlowingDistance = 15f;
    public float stoppingDistance = 0.05f;
    private float currentSpeed = 0f;

    [Header("Jib Settings")]
    public GameObject jibObject;
    public float jibMaxSpeed = 0.02f;
    public float jibSlowingDistance = 1f;

    [Header("Hooking Unit Settings")]
    public GameObject hookingUnit;
    public Transform hookingUnitRestPosition;
    public float hookedUnitOffset = 0.125f;
    public float hookAcceleration = 0.1f;
    public float unitMoveSpeed = 0.1f;

    private GameObject hookedObject;
    private bool hasObject = false;

    public HookingUnitCable[] cables;

    private Vector3 targetLocation;
    private int xIndex = 0, zIndex = 0;

    [Header("Idle Settings")]
    public float idleWaitTime = 10f;
    private float nextWaitTime = 0f;
    private float timer = 0f;

    [HideInInspector]
    public ShippingContainerSpawner parent;

    private Vector3Int containerAreaSize;
    private Vector3 containerSize;

    private void Start()
    {
        nextWaitTime = Random.Range(idleWaitTime / 2f, idleWaitTime * 5f);
        
        containerAreaSize = parent.containerAreaSize;
        containerSize = parent.GetContainerSize();
        ResetLocation();
    }

    private void Update()
    {
        if (currentState == GantryState.Idle)
        {
            timer += Time.deltaTime;

            if (timer >= nextWaitTime)
            {
                ResetLocation();
            }
        }
        else
        {
            if (currentState == GantryState.Moving)
            {
                // The entire gantry is not over the row of stacks we want.
                MoveObjectToTarget(gameObject, AxisConstraint.ZAxis, gantrySlowingDistance, gantryMaxSpeed, GantryState.GantryInPosition);
            }
            else if (currentState == GantryState.GantryInPosition)
            {
                // Gantry is over the row, but we need to move the jib over the specific Stack
                MoveObjectToTarget(jibObject, AxisConstraint.XAxis, jibSlowingDistance, jibMaxSpeed, GantryState.JibInPosition);
            }
            else if (currentState == GantryState.JibInPosition)
            {
                //currentState = GantryState.Idle;

                ContainerStack stack = parent.GetContainerStack(xIndex, zIndex);
                //Debug.LogFormat("The chosen ContainerStack contains {0} Containers", stack.stack.Count);

                // The jib unit is over the correct stack, here is where we want to check to see if 
                // there are any containers in the stack and if the crane currently has a container lifted.
                if (stack.stack.Count == 0)
                {
                    if (hasObject)
                    {
                        // Drop it on the floor.
                        float height = parent.transform.position.y + containerSize.y + (hookedUnitOffset * 2f);
                        StartCoroutine(DropContainer(height, stack, hookedObject));
                    }
                    else
                    {
                        // Enter the idle state
                        currentState = GantryState.Idle;
                    }
                }
                else if (stack.stack.Count >= 5)
                {
                    currentState = GantryState.Idle;
                }
                else
                {
                    if (hasObject)
                    {
                        // If there is a container lifted, then we want to drop it on top on the stack.
                        float height = stack.stack[stack.stack.Count - 1].transform.position.y + (containerSize.y * 2f) + (hookedUnitOffset * 2f);
                        StartCoroutine(DropContainer(height, stack, hookedObject));
                    }
                    else
                    {
                        // If there is no container lifted: Pick up the top object.
                        if (currentState != GantryState.HookingUnitMoving)
                        {
                            GameObject target = stack.stack[stack.stack.Count - 1];

                            if (target == null)
                            {
                                currentState = GantryState.Idle;
                                return;
                            }

                            float height = target.transform.position.y + containerSize.y + (hookedUnitOffset * 2f);

                            StartCoroutine(HookContainer(height, stack, target));
                        }
                    }
                }

            }
        }

        UpdateCables();
    }

    private void UpdateCables()
    {
        foreach(HookingUnitCable cable in cables)
        {
            Vector3[] positions = new Vector3[] { cable.lineRenderer.gameObject.transform.position, cable.endPosition.position };

            cable.lineRenderer.SetPositions(positions);
        }
    }

    private void ResetLocation()
    {
        targetLocation = ChooseNewPosition();
    }

    private enum AxisConstraint { XAxis, ZAxis};

    /// <summary>
    /// This moves an object to a desired location with constraints on either the X/Z Axis AND the Y Axis. The object moves with acceleration and deceleration.
    /// </summary>
    /// <param name="toMove"> The GameObject that we want to move. </param>
    /// <param name="constraint"> The axis we want to to additionally constrain. </param>
    /// <param name="slowingDistance"> Radius at which we want to start slowing down the object. </param>
    /// <param name="maxSpeed"> Maximum speed that the object can reach while moving. </param>
    /// <param name="stateOnCompletion"> The State in which we want the Gantry to enter on move completion. </param>
    private void MoveObjectToTarget(GameObject toMove, AxisConstraint constraint, float slowingDistance, float maxSpeed, GantryState stateOnCompletion)
    {
        Vector3 desiredVelocity = targetLocation - toMove.transform.position;
        desiredVelocity.y = 0f;

        if (constraint == AxisConstraint.XAxis)
        {
            desiredVelocity.x = 0f;
        }
        else
        {
            desiredVelocity.z = 0f;
        }

        float distanceToTarget = desiredVelocity.magnitude;

        if (distanceToTarget <= stoppingDistance)
        {
            currentSpeed = 0f;
            currentState = stateOnCompletion;
            return;
        }
        else if (distanceToTarget < slowingDistance)
        {
            desiredVelocity = desiredVelocity.normalized * currentSpeed * Mathf.Max(0.005f, (distanceToTarget / slowingDistance));
        }
        else
        {
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += maxAcceleration;
            }

            desiredVelocity = desiredVelocity.normalized * currentSpeed;
        }

        toMove.transform.position += parent.transform.rotation * desiredVelocity * Time.deltaTime;
    }

    private IEnumerator HookContainer(float desiredHeight, ContainerStack stack, GameObject toPickup)
    {
        currentState = GantryState.HookingUnitMoving;

        // In here we want to move the hooking unit down to a given height above the ground, set the object to pick up to be a child of the unit, and raise the unit back to it's rest position
        Vector3 targetPos = targetLocation;
        targetPos.y = desiredHeight;

        float remainingDistance = Vector3.Distance(hookingUnit.transform.position, targetPos);
        float currentSpeed = 0f;

        while (remainingDistance >= stoppingDistance * 2.5f)
        {
            if (remainingDistance > jibSlowingDistance)
            {
                if (currentSpeed < unitMoveSpeed)
                {
                    currentSpeed += hookAcceleration;
                }
            }
            else
            {
                currentSpeed = unitMoveSpeed * (remainingDistance / jibSlowingDistance);
            }


            remainingDistance = Vector3.Distance(hookingUnit.transform.position, targetPos);

            // Move the unit down
            Vector3 newPosition = hookingUnit.transform.position;
            newPosition.y -= currentSpeed * Time.deltaTime;

            hookingUnit.transform.position = newPosition;

            yield return null;
        }
        currentSpeed = 0f;
        yield return new WaitForSeconds(2f);

        // Now that we are at the target location, we want to "Grab" the object.
        // We are going to remove the container from the ContainerStack it was stored in.
        stack.stack.Remove(toPickup);

        toPickup.transform.parent = hookingUnit.transform;

        Vector3 hookedPosition = new Vector3(0f, -(containerSize.y + (hookedUnitOffset * 2f)), 0f);
        toPickup.transform.localPosition = hookedPosition;

        hookedObject = toPickup;
        hasObject = true;

        // Now that we have grabbed the object, we want to move the hooking unit back up.
        remainingDistance = Vector3.Distance(hookingUnit.transform.position, hookingUnitRestPosition.position);

        while (remainingDistance >= stoppingDistance)
        {
            if (remainingDistance > jibSlowingDistance)
            {
                if (currentSpeed < unitMoveSpeed)
                {
                    currentSpeed += hookAcceleration;
                }
            }
            else
            {
                currentSpeed = unitMoveSpeed * (remainingDistance / jibSlowingDistance);
            }

            remainingDistance = Vector3.Distance(hookingUnit.transform.position, hookingUnitRestPosition.position);

            Vector3 newPosition = hookingUnit.transform.position;
            newPosition.y += currentSpeed * Time.deltaTime;

            hookingUnit.transform.position = newPosition;

            yield return null;
        }

        hookingUnit.transform.position = hookingUnitRestPosition.position;

        nextWaitTime = idleWaitTime;

        currentState = GantryState.Idle;
    }

    private IEnumerator DropContainer(float desiredHeight, ContainerStack stack, GameObject toPickup)
    {
        currentState = GantryState.HookingUnitMoving;

        // In here we want to move the hooking unit down to a given height above the ground, set the object to pick up to be a child of the unit, and raise the unit back to it's rest position
        Vector3 targetPos = targetLocation;
        targetPos.y = desiredHeight;

        float remainingDistance = Vector3.Distance(hookingUnit.transform.position, targetPos);
        float currentSpeed = 0f;

        while (remainingDistance >= stoppingDistance * 2.5f)
        {
            if (remainingDistance > jibSlowingDistance)
            {
                if (currentSpeed < unitMoveSpeed)
                {
                    currentSpeed += hookAcceleration;
                }
            }
            else
            {
                currentSpeed = unitMoveSpeed * (remainingDistance / jibSlowingDistance);
            }

            remainingDistance = Vector3.Distance(hookingUnit.transform.position, targetPos);

            // Move the unit down
            Vector3 newPosition = hookingUnit.transform.position;
            newPosition.y -= currentSpeed * Time.deltaTime;

            hookingUnit.transform.position = newPosition;

            yield return null;
        }

        currentSpeed = 0f;
        yield return new WaitForSeconds(2f);

        // Now we want to un-parent the container, set everything relating to holding it to false and null and add the container to the
        // corresponding stack
        toPickup.transform.parent = parent.GetContainerParent().transform;
        hookedObject = null;
        hasObject = false;

        stack.stack.Add(toPickup);

        // Now that we have grabbed the object, we want to move the hooking unit back up.
        remainingDistance = Vector3.Distance(hookingUnit.transform.position, hookingUnitRestPosition.position);

        while (remainingDistance >= stoppingDistance)
        {
            if (remainingDistance > jibSlowingDistance)
            {
                if (currentSpeed < unitMoveSpeed)
                {
                    currentSpeed += hookAcceleration;
                }
            }
            else
            {
                currentSpeed = unitMoveSpeed * (remainingDistance / jibSlowingDistance);
            }

            remainingDistance = Vector3.Distance(hookingUnit.transform.position, hookingUnitRestPosition.position);

            Vector3 newPosition = hookingUnit.transform.position;
            newPosition.y += currentSpeed * Time.deltaTime;

            hookingUnit.transform.position = newPosition;

            yield return null;
        }

        hookingUnit.transform.position = hookingUnitRestPosition.position;

        nextWaitTime = Random.Range(idleWaitTime / 2f, idleWaitTime * 5f);

        currentState = GantryState.Idle;
    }

    private Vector3 ChooseNewPosition()
    {
        timer = 0f;

        currentState = GantryState.Moving;

        // We want to create a random index on the X and Z axis.
        // This will choose a stack which we want to access
        int randX, randZ;

        xIndex = randX = Random.Range(0, containerAreaSize.x);
        zIndex = randZ = Random.Range(0, containerAreaSize.z);

        return parent.GetStackLocation(randX, randZ);
    }

    public void SetContainerInfo(Vector3Int areaSize, Vector3 containerSize)
    {
        containerAreaSize = areaSize;
        this.containerSize = containerSize;
    }
}

[System.Serializable]
public class HookingUnitCable
{
    public LineRenderer lineRenderer;
    public Transform endPosition;
}

public enum GantryState
{
    GantryInPosition, JibInPosition, Moving, HookingUnitMoving, Idle
}
