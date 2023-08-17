using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoredObject : MonoBehaviour
{
    public float objectValue = 1000f;
    private float remainingValue = 0f;

    private void Start()
    {
        remainingValue = objectValue;
    }

    /// <summary>
    /// Awards the player with a proportion of the objects remaining value based on the force applied to the object.
    /// </summary>
    /// <param name="forceApplied"> The force which has been applied to the object. </param>
    public void AwardValue(float forceApplied)
    {
        float cost, forceScale = 1000f;

        cost = objectValue * (forceApplied / forceScale);

        if (remainingValue - cost <= 0f)
        {
            cost = remainingValue;
            remainingValue = 0f;
        }
        else
        {
            remainingValue -= cost;
        }

        GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, cost));
    }

    /// <summary>
    /// Awards the player with the total remaining value of the object. Typically used when an object is vapourised.
    /// </summary>
    public void AwardAllValue()
    {
        if (remainingValue != 0f)
        {
            GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, remainingValue));
            remainingValue = 0f;
        }
    }
}
