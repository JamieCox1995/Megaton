using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
    private Stack<TimeScaleAdjustmentEvent.Settings> _timeScaleAdjustmentStack;

    public static TimeScaleManager instance;

    private const float FixedDeltaTimeFactor = 0.02f;

    private Queue<Transition> _transitionQueue;

    private float _currentTimeScale = 1f;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            this.enabled = false;
            return;
        }

        _timeScaleAdjustmentStack = new Stack<TimeScaleAdjustmentEvent.Settings>();
        _transitionQueue = new Queue<Transition>();

        GameEventManager.instance.onStartTimeScaleAdjustment += StartTimeScaleAdjustment;
        GameEventManager.instance.onEndTimeScaleAdjustment += EndTimeScaleAdjustment;

        StartCoroutine(ProcessTransitionQueue());
    }

    private void EndTimeScaleAdjustment()
    {
        if (_timeScaleAdjustmentStack.Count == 0) return;

        TimeScaleAdjustmentEvent.Settings settings = _timeScaleAdjustmentStack.Pop();

        if (!settings.hasTransition)
        {
            _currentTimeScale = PeekTimeScale();
            SetTimeScale(PeekTimeScale());
            return;
        }

        float timeScaleEnd = PeekTimeScale();

        Transition transition = new Transition
        {
            timeScaleStart = settings.timeScale,
            timeScaleEnd = timeScaleEnd,
            easing = settings.easing,
            duration = settings.transitionTime,
        };

        QueueTransition(transition);
    }

    private void StartTimeScaleAdjustment(TimeScaleAdjustmentEvent obj)
    {
        float timeScaleStart = PeekTimeScale();

        _timeScaleAdjustmentStack.Push(obj.settings);

        if (!obj.settings.hasTransition)
        {
            SetTimeScale(obj.settings.timeScale);
            return;
        }

        Transition transition = new Transition
        {
            timeScaleStart = timeScaleStart,
            timeScaleEnd = obj.settings.timeScale,
            easing = obj.settings.easing,
            duration = obj.settings.transitionTime,
        };

        QueueTransition(transition);
    }

    private void QueueTransition(Transition transition)
    {
        _transitionQueue.Enqueue(transition);
    }

    private IEnumerator ProcessTransitionQueue()
    {
        while (true)
        {
            if (_transitionQueue.Count > 0)
            {
                yield return StartCoroutine(DoTransition(_transitionQueue.Dequeue()));
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator DoTransition(Transition transition)
    {
        _currentTimeScale = transition.timeScaleEnd;

        float timer = 0f;

        while (timer < transition.duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / transition.duration;

            t = transition.easing.Evaluate(t);

            float timeScale = Mathf.Lerp(transition.timeScaleStart, transition.timeScaleEnd, t);

            SetTimeScale(timeScale);

            yield return null;
        }

        SetTimeScale(transition.timeScaleEnd);
    }

    private void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = timeScale * FixedDeltaTimeFactor;
    }

    public float GetCurrentTimeScale()
    {
        return _currentTimeScale;
    }

    private float PeekTimeScale()
    {
        if (_timeScaleAdjustmentStack.Count == 0) return 1f;
        return _timeScaleAdjustmentStack.Peek().timeScale;
    }

    public float GetCurrentFixedDeltaTime()
    {
        return GetCurrentTimeScale() * FixedDeltaTimeFactor;
    }

    private class Transition
    {
        public float timeScaleStart;
        public float timeScaleEnd;
        public AnimationCurve easing;
        public float duration;
    }
}
