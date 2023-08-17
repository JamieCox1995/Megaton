using System;
using System.Collections;
using System.Collections.Generic;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public abstract class ActionTutorialStep : TutorialStep
{
    public override IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        Setup();

        yield return PerformAction();

        Cleanup();
    }

    public virtual void Setup()
    {
        ServiceLocator.Get<IInputManager>().DisableAllAxes();
    }

    public virtual void Cleanup()
    {
        ServiceLocator.Get<IInputManager>().EnableAllAxes();
    }

    public abstract IEnumerator PerformAction();
}
