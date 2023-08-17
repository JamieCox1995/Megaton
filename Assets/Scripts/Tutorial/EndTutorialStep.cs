using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTutorialStep : TutorialStep
{
    public override bool RequiresCursorControl { get { return true; } }

    public override IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        using (new PauseGameScope())
        {
            yield return WaitForButton(tutorialUI);
        }
    }

    private IEnumerator WaitForButton(CanvasGroup tutorialUI)
    {
        yield return tutorialUI.AddButton("Close");
    }
}
