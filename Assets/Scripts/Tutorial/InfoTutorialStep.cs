using System;
using System.Collections;
using System.Collections.Generic;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public class InfoTutorialStep : TutorialStep
{
    public bool PauseGame;
    public bool DisableInputs = true;
    public bool UseHighlight;
    public AnchorPresets HighlightAnchor = AnchorPresets.MiddleCenter;
    public Rect HighlightRect;
    public Color HighlightColor;

    public override bool RequiresCursorControl { get { return true; } }

    public override IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        using (this.PauseGame ? new PauseGameScope() : null)
        {
            if (this.UseHighlight) tutorialUI.SetHighlightAnchor(this.HighlightAnchor).HighlightRect(this.HighlightRect, this.HighlightColor);

            if (this.DisableInputs)
            {
                ServiceLocator.Get<IInputManager>().DisableAllAxes();
                ServiceLocator.Get<IInputManager>().EnableAxis("Pause");
                ServiceLocator.Get<IInputManager>().EnableAxis("Submit");
            }

            yield return WaitForButton(tutorialUI);

            if (this.DisableInputs)
            {
                ServiceLocator.Get<IInputManager>().EnableAllAxes();
            }

            if (this.UseHighlight) tutorialUI.SetHighlightAnchor(AnchorPresets.MiddleCenter).HideHighlight();
        }
    }

    private IEnumerator WaitForButton(CanvasGroup tutorialUI)
    {
        yield return tutorialUI.AddButton("Next");
    }
}
