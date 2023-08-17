using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForSecondsTutorialStep : TutorialStep
{
    public float WaitTime;

    public override IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        yield return new WaitForSeconds(this.WaitTime);
    }
}
