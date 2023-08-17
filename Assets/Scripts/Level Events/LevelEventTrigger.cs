using UnityEngine;

public class LevelEventTrigger<TEvent> : MonoBehaviour
        where TEvent : LevelEvent
{
    public TEvent LevelEvent;

    protected virtual void OnCollisionEnter(Collision collision) { }
    protected virtual void OnTriggerEnter(Collider other) { }
}