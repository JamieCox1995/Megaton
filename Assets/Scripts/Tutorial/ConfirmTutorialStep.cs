using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmTutorialStep : TutorialStep
{
    public List<string> ButtonTexts;
    public int CancelButtonIndex;

    public override bool RequiresCursorControl { get { return true; } }

    public override IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        Coroutine<int> coroutine;

        using (new PauseGameScope())
        {
            coroutine = tutorialUI.AddButtons(this.ButtonTexts.ToArray());
            yield return coroutine.Start(tutorial);
        }

        if (coroutine.Value == this.CancelButtonIndex) yield return Result.Break;       
    }
}
