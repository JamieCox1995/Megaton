using System;
using System.Diagnostics;
using UnityEngine;

public class FrameTimer : MonoBehaviour
{
    private int _id;
    private static int _counter;

    private void Awake()
    {
        _id = _counter++;
        if (_id == 0)
        {
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    [RuntimeInitializeOnLoadMethod]
    static void OnLoad()
    {
        new GameObject("FrameTimer").AddComponent<FrameTimer>();
    }

    // Update is called once per frame
    void Update () {
        if (_id == 0) FrameTimerInternal.Restart();
	}

    public static float TimeSinceFrameStart { get { return (float)FrameTimerInternal.Elapsed.TotalSeconds; } }

    private static class FrameTimerInternal
    {
        private static Stopwatch _stopwatch;

        static FrameTimerInternal()
        {
            _stopwatch = new Stopwatch();
        }

        public static void Restart()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        public static TimeSpan Elapsed { get { return _stopwatch.Elapsed; } }
    }
}