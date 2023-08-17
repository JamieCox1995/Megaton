using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventTutorialStep : TutorialStep
{
    private bool _isHandled;
    public bool IsHandled { get { return _isHandled; } }

    public override IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        yield return WaitForHandledEvent();
    }

    private IEnumerator WaitForHandledEvent()
    {
        Clear();

        Setup();

        RegisterEventHandler();

        while (!this.IsHandled) yield return null;

        DeregisterEventHandler();

        Cleanup();
    }

    protected abstract void RegisterEventHandler();
    protected abstract void DeregisterEventHandler();

    protected virtual void Setup() { }
    protected virtual void Cleanup() { }

    private void Clear()
    {
        _isHandled = false;
    }

    protected void MarkAsHandled()
    {
        _isHandled = true;
    }
}
