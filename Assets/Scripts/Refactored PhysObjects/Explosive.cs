using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Explosive Settings:")]
    public ExplosiveSettings _explosiveSettings;

    [Header("Additional Settings:")]
    public bool destroyGameObjectOnExplode = false;
    public Transform explosionOverridePosition;

    public void TriggerExplosion()
    {
        if (_explosiveSettings.hasExploded == false) Explode();
    }

    private void Explode()
    {
        Vector3 explosionPosition = explosionOverridePosition == null ? transform.position : explosionOverridePosition.position;

        GameEventManager.instance.TriggerEvent(new ExplosionEvent(GameEventType.OnExplosion, explosionPosition, _explosiveSettings.explosiveForce));
        Instantiate(_explosiveSettings.explosionPrefab, explosionPosition, Quaternion.identity);

        _explosiveSettings.hasExploded = true;

        if (destroyGameObjectOnExplode) Destroy(gameObject);
    }
}
