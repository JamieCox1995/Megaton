using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class OptimalDistanceVisuliser : MonoBehaviour {

    CinemachineCollider colliderComponent;

    private void OnDrawGizmos()
    {
        if (colliderComponent == null)
        {
            colliderComponent = GetComponent<CinemachineCollider>();
        }

        if (colliderComponent.m_OptimalTargetDistance == 0) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, colliderComponent.m_OptimalTargetDistance);
    }

}
