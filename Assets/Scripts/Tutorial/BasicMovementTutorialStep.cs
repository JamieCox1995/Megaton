using System;
using System.Collections;
using System.Collections.Generic;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public class BasicMovementTutorialStep : TutorialStep
{
    public string InputAxis;
    public float MinimumInputTime;

    public override IEnumerator Complete(Tutorial tutorial, CanvasGroup tutorialUI)
    {
        yield return WaitForAxis(this.InputAxis, this.MinimumInputTime);
    }

    private IEnumerator WaitForAxis(string axisName, float minInputTime)
    {
        bool completePositiveMovement = false;
        bool completeNegativeMovement = false;

        float timePositive = 0f;
        float timeNegative = 0f;

        ServiceLocator.Get<IInputManager>().DisableAllAxes();
        ServiceLocator.Get<IInputManager>().EnableAxis(axisName);

        while (!(completePositiveMovement && completeNegativeMovement))
        {
            if (ServiceLocator.Get<IInputProxyService>().GetAxis(axisName) > 0)
            {
                timePositive += Time.deltaTime;
                Debug.LogFormat("timePositive: {0}", timePositive);
            }
            else if (ServiceLocator.Get<IInputProxyService>().GetAxis(axisName) < 0)
            {
                timeNegative += Time.deltaTime;

                Debug.LogFormat("timeNegative: {0}", timeNegative);
            }
            else
            {
                if (timePositive < minInputTime)
                {
                    timePositive = 0f;
                }
                else
                {
                    completePositiveMovement = true;
                }

                if (timeNegative < minInputTime)
                {
                    timeNegative = 0f;
                }
                else
                {
                    completeNegativeMovement = true;
                }
            }

            yield return null;
        }

        ServiceLocator.Get<IInputManager>().EnableAllAxes();
    }
}
