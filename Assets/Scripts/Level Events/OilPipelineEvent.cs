using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilPipelineEvent : LevelEvent
{
    [Header("Oil Pipeline 1:")]
    public MotionPathRunner runner1;
    public float duration1 = 30f;

    [Header("Oil Pipeline 2:")]
    public MotionPathRunner runner2;
    public float duration2 = 20f;

    public Animator _Animator;
    private int _track;
    private bool _Triggered = false;

    public void OnEventTriggered(int track)
    {
        if (_Triggered == false)
        {
            _track = track;

            StartLevelEvent();
        }
    }

    protected override void OnEventStart()
    {   
        if (_track == 0)
        {
            runner1.StartRunner();
        }
        else if (_track == 1)
        {
            runner2.StartRunner();
        }

        eventDuration = (_track == 0) ? duration1 : duration2;

        _Animator.SetInteger("CameraTrack", _track);
        _Triggered = true;
    }

    protected override void OnEventEnd()
    {
        if (_track == 0)
        {
            runner1.StopRunner();
        }
        else if (_track == 1)
        {
            runner2.StopRunner();
        }
    }
}
