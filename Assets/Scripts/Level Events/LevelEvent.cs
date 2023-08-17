using System.Collections;
using UnityEngine;

public class LevelEvent : MonoBehaviour
{
    public float eventDuration;
    public Transform cameraOverride;

    protected bool EventTriggered { get; private set; }

    public void StartLevelEvent()
    {
        StartCoroutine(Invoke());
    }

    protected virtual void OnEventStart() { }
    protected virtual void OnEventEnd() { }

    protected virtual IEnumerator DoEvent()
    {
        yield return new WaitForSeconds(eventDuration);
    }

    private IEnumerator Invoke()
    {
        if (this.EventTriggered) yield break;

        Setup();

        yield return DoEvent();

        TearDown();
    }

    private void Setup()
    {
        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnLevelEventStarted));

        this.EventTriggered = true;

        OnEventStart();
    }

    private void TearDown()
    {
        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnLevelEventEnded));

        OnEventEnd();
    }
}
