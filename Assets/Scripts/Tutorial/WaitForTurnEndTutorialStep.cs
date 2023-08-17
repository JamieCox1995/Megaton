using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForTurnEndTutorialStep : EventTutorialStep
{
    protected override void RegisterEventHandler()
    {
        GameEventManager.instance.onTurnEnd += TurnEndHandler;
    }

    protected override void DeregisterEventHandler()
    {
        GameEventManager.instance.onTurnEnd -= TurnEndHandler;
    }

    void TurnEndHandler()
    {
        MarkAsHandled();
    }
}
