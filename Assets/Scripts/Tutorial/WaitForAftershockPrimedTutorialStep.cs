using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForAftershockPrimedTutorialStep : EventTutorialStep
{
    protected override void RegisterEventHandler()
    {
        GameEventManager.instance.onAftershockPrimed += AftershockPrimedHandler;
    }

    protected override void DeregisterEventHandler()
    {
        GameEventManager.instance.onAftershockPrimed -= AftershockPrimedHandler;
    }

    private void AftershockPrimedHandler()
    {
        MarkAsHandled();
    }
}
