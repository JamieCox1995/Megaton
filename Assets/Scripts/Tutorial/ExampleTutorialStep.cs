using System.Collections;
using System.Collections.Generic;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

[AddComponentMenu("Tutorial/Example Tutorial Step", 1)]
public class ExampleTutorialStep : TutorialStep
{
    public override IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        while (!ServiceLocator.Get<IInputProxyService>().anyKeyDown) yield return null;
    }
}
