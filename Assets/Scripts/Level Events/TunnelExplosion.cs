using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelExplosion : LevelEvent
{
    [Header("Explosion Settings")]
    public float explosionAngle;
    public float explosionForce;

    public GameObject effect;
    private GameObject _spawnedEffect;
	
    public void OnEventTriggered()
    {
        StartLevelEvent();
    }

    protected override void OnEventStart()
    {
        FindObjectOfType<ChunkManager>().ShapedExplosion(transform, explosionAngle, explosionForce);

        GameEventManager.instance.TriggerEvent(new TimeScaleAdjustmentEvent(GameEventType.OnStartTimeScaleAdjustment, 1f));
        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnAftershockExited));

        _spawnedEffect = Instantiate(effect, transform.position, transform.rotation);
    }

    protected override void OnEventEnd()
    {
        Destroy(_spawnedEffect);
    }
}
