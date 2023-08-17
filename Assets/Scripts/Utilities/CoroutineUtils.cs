using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineUtils
{
    public static Coroutine StartCoroutineAndMatchTargetFrameRate(this MonoBehaviour obj, IEnumerator routine)
    {
        int targetFrameRate = Application.targetFrameRate;

        return obj.StartCoroutine(MatchTargetFrameRateCoroutine(routine, targetFrameRate));
    }

    private static IEnumerator MatchTargetFrameRateCoroutine(IEnumerator routine, int targetFrameRate)
    {
        float targetTime = 0.95f / targetFrameRate;

        while (true)
        {
            do
            {
                if (!routine.MoveNext()) yield break;
            } while (FrameTimer.TimeSinceFrameStart < targetTime);

            yield return routine.Current;
        }
    }

    public static Coroutine<T> StartCoroutine<T>(this MonoBehaviour obj, IEnumerator routine)
    {
        Coroutine<T> coroutine = new Coroutine<T>(routine);

        coroutine.Start(obj);

        return coroutine;
    }

    public static TrackableCoroutine AsTrackable(this IEnumerator routine)
    {
        return new TrackableCoroutine(routine);
    }

    public static IEnumerator MatchTargetFrameRate(this IEnumerator routine)
    {
        int targetFrameRate = Application.targetFrameRate;

        return MatchTargetFrameRateCoroutine(routine, targetFrameRate);
    }

    public static Coroutine<T> OfType<T>(this IEnumerator routine)
    {
        return new Coroutine<T>(routine);
    }
}

public abstract class ExtendedCoroutine : IEnumerator
{
    private IEnumerator _baseRoutine;

    private IEnumerator _extendedRoutine;

    public ExtendedCoroutine(IEnumerator routine)
    {
        _baseRoutine = routine;
        _extendedRoutine = InternalRoutine(_baseRoutine);
    }

    public object Current
    {
        get
        {
            return _extendedRoutine.Current;
        }
    }

    public bool MoveNext()
    {
        return _extendedRoutine.MoveNext();
    }

    public void Reset()
    {
        _extendedRoutine.Reset();
    }

    public Coroutine Start(MonoBehaviour obj)
    {
        return obj.StartCoroutine(_extendedRoutine);
    }

    protected abstract IEnumerator InternalRoutine(IEnumerator routine);
}

public class TrackableCoroutine : ExtendedCoroutine
{

    public bool IsRunning { get; private set; }

    public TrackableCoroutine(IEnumerator routine) : base(routine) { }

    protected override IEnumerator InternalRoutine(IEnumerator routine)
    {
        this.IsRunning = true;

        while (routine.MoveNext())
        {
            yield return routine.Current;
        }

        this.IsRunning = false;
    }
}

public class Coroutine<T> : ExtendedCoroutine
{
    public T Value { get; private set; }

    public Coroutine(IEnumerator routine) : base(routine) { }

    protected override IEnumerator InternalRoutine(IEnumerator routine)
    {
        while (true)
        {
            if (!routine.MoveNext()) yield break;

            object yielded = routine.Current;

            if (yielded != null && yielded is T)
            {
                this.Value = (T)yielded;
                yield break;
            }
            else
            {
                yield return routine.Current;
            }
        }
    }
}