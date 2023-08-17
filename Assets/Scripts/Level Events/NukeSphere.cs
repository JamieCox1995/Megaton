using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeSphere : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FracturedChunk chunk = other.gameObject.GetComponent<FracturedChunk>();

        if (chunk != null)
        {
            if (chunk.IsDestructibleChunk())
            {
                chunk.FracturedObjectSource.NotifyChunkDetach(chunk);

                AddScoreOnDamage asod = chunk.GetComponent<AddScoreOnDamage>();

                asod.DealAllDamage();

                chunk.gameObject.SetActive(false);
            }
        }
        else
        {
            PhysicsObject physObj = other.GetComponent<PhysicsObject>();

            if (physObj != null)
            {
                physObj.VapouriseObject();
            }
        }
    }
}
