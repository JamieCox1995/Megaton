using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ExplosiveOld
{
    private ExplosiveSettings settings;

    public ExplosiveOld(ExplosiveSettings s)
    {
        settings = s;
    }

    // Explode is used to effect other in-game objects
    public void Explode(Vector3 position)
    {
        GameEventManager.instance.TriggerEvent(new ExplosionEvent(GameEventType.OnExplosion, position, settings.explosiveForce));

        GameObject.Instantiate(settings.explosionPrefab, position, Quaternion.identity);

        settings.hasExploded = true;
    }

    public bool HasExploded()
    {
        return settings.hasExploded;
    }
}

[System.Serializable]
public class ExplosiveSettings
{
    [FormerlySerializedAs("explosiveSize")]
    public float explosiveForce = 10f;
    //public float explosiveRadius = 50f;

    public LayerMask objectsToEffect;
    public GameObject explosionPrefab;

    public bool hasExploded = false;
}
