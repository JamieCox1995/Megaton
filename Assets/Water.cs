using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [SerializeField]
    private GameObject projectileSplash;

    [SerializeField]
    private GameObject[] objectSplashes;
    private List<Collider> _currentlySubmerged;

    [SerializeField]
    private LayerMask splashLayerMask;

	// Use this for initialization
	void Start ()
    {
        _currentlySubmerged = new List<Collider>();	
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            // Spawn in effect
            Instantiate(projectileSplash, GetSplashPosition(other.GetComponentInParent<Rigidbody>()), Quaternion.identity);

            if (!other.gameObject.GetComponentInParent<Aftershock>())
            {
                // We want to destroy the projectile
                Destroy(other.transform.parent.gameObject);

                // We also want to call the GameEvent that causes the players' turn to end
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnProjectileDestroyed));
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Projectile"))
        {
            Rigidbody _RigidBody = other.gameObject.GetComponent<Rigidbody>();

            if (_RigidBody != null && _RigidBody.velocity != Vector3.zero)
            {
                if (!_currentlySubmerged.Contains(other))
                {
                    // Add the collider to the list
                    _currentlySubmerged.Add(other);

                    // Spawn in a random splashEffect
                    int randInt = Random.Range(0, objectSplashes.Length);

                    Vector3 pos = GetSplashPosition(other.gameObject.GetComponent<Rigidbody>());

                    //Debug.LogFormat("There was a splash at {0}", GetSplashPosition(other.gameObject.GetComponent<Rigidbody>()));

                    Instantiate(objectSplashes[randInt], pos, Quaternion.identity);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_currentlySubmerged.Contains(other)) _currentlySubmerged.Remove(other);
    } 

    private Vector3 GetSplashPosition(Rigidbody rigid)
    {
        Vector3 splashPoint = Vector3.zero;

        RaycastHit hit;

        if (Physics.Raycast(rigid.transform.position, rigid.velocity.normalized, out hit, 50f, splashLayerMask))
        {
            splashPoint = hit.point;
        }
        else if (Physics.Raycast(rigid.transform.position, -rigid.velocity.normalized, out hit, 50f, splashLayerMask))
        {
            splashPoint = hit.point;
        }

        return splashPoint;
    }
}
