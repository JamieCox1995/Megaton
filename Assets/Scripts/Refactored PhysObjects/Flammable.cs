using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Flammable : MonoBehaviour
{
    public float burnTime;
    private float timer = 0f;
    [Tooltip("Holds all of the transforms to be used as points of burning. Transforms are used so that we can also use the direction of the object to orientate the effect.")]
    public List<Transform> burnPosition;
    public GameObject burningEffect;
    private bool burning = false;

    public bool applyThurstOnBurn = true;
    public float thurstForce = 10f;

    public UnityEvent onBurningCompleted; 
    
    public void StartBurning()
    {
        if (burning == false)
        {
            StartCoroutine(Burn());
        }
    }

    public void StopBurning()
    {
        if (burning == true)
        {
            StopCoroutine(Burn());
        }
    }

    private IEnumerator Burn()
    {
        burning = true;

        // Spawning the burning effect at all of the burn positions
        foreach(Transform burnPos in burnPosition)
        {
            GameObject effect = Instantiate(burningEffect, burnPos.position, Quaternion.identity, burnPos);
            effect.transform.forward = burnPos.forward;
        }

        // Now we are checking to see if we want to apply a force to the object whilst it is burning. If we are not going to
        // apply a force to the object, we are just going to make this Coroutine to wait for the burn time.
        timer = 0f;

        // Grabbing the rigidbody on the object.
        Rigidbody _rigidbody = GetComponent<Rigidbody>();

        while (timer <= burnTime)
        {
            timer += Time.deltaTime;

            if (applyThurstOnBurn)
            {
                foreach (Transform burnPos in burnPosition)
                {
                    // Applying a force to the rigidbody at the location of the burnPosition
                    _rigidbody.AddForceAtPosition(-burnPos.forward * thurstForce, burnPos.position, ForceMode.Acceleration);
                }
            }

            yield return null;
        }


        onBurningCompleted.Invoke();
    }
}
