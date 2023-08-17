using System;
using System.Collections;
using System.Collections.Generic;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public abstract class TutorialStep : MonoBehaviour
{
    public List<string> PanelText;
    public Rect PanelRect;

    public virtual bool RequiresCursorControl { get { return false; } }

    public IEnumerator Run(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        tutorialUI.Clear();
        tutorialUI.Show();
        tutorialUI.SetTextPanelRect(this.PanelRect);

        foreach (string paragraph in this.PanelText)
        {
            tutorialUI.AddRichTextParagraph(paragraph.FormatGameConcepts(tutorial.GameConceptFormat));
        }

        if (this.RequiresCursorControl) ServiceLocator.Get<IInputManager>().DisableAxis("Launch");

        Coroutine<Result> coroutine = new Coroutine<Result>(Complete(tutorial, tutorialUI));

        yield return coroutine.Start(tutorial);

        if (this.RequiresCursorControl) ServiceLocator.Get<IInputManager>().EnableAxis("Launch");

        if (coroutine.Value == Result.Break) yield return Result.Break;
    }

    public abstract IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI);

    public enum Result
    {
        Continue,
        Break,
    }

    protected class PauseGameScope : IDisposable
    {
        public PauseGameScope()
        {
            GameEventManager.instance.TriggerEvent(new TimeScaleAdjustmentEvent(GameEventType.OnStartTimeScaleAdjustment, 0f));
        }

        public void Dispose()
        {
            GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnEndTimeScaleAdjustment));
        }
    }
}