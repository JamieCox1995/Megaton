using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddScoreOnDamage : MonoBehaviour
{
    public ObjectSettings objectSettings;
    public float vapourisationForce = 150f;

    private float remainingValue = 0f;

	// Use this for initialization
	void Start ()
    {
        remainingValue = objectSettings.startingValue;
	}

    public float GetValue()
    {
        return objectSettings.startingValue;
    }

    public void SetObjectSettings(float startingValue, float damageThreshold, float vapourisationForce = 200f)
    {
        objectSettings = new ObjectSettings(startingValue, damageThreshold);

        this.vapourisationForce = vapourisationForce;
    }

    public void DealDamage(float force)
    {
        // If the force from the explosion is enough to vapourise the object first, so that we can
        // destroy the object without having to calculate anything else.
        if (Mathf.Abs(force) >= vapourisationForce)
        {
            GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, remainingValue));
            Destroy(gameObject);
        }

        float cost = objectSettings.startingValue * (force / 1000f);

        if (remainingValue - cost <= 0f)
        {
            cost = remainingValue;

            remainingValue = 0f;
        }
        else
        {
            remainingValue -= cost;
        }

        //Debug.Log("Taken damage, updating Score with: " + cost);

        GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, cost));
    }

    public void CheckForVapourisation(float force)
    {
        if (Mathf.Abs(force) >= vapourisationForce)
        {
            DealAllDamage();

            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    public void DealAllDamage()
    {
        // As we're not sure if the object has already taken some damage, we will just using the remainingValue to update the score.
        GameEventManager.instance.TriggerEvent(new ScoreUpdateEvent(GameEventType.OnScoreUpdated, remainingValue));
    }

    public float GetRemainingValue()
    {
        return remainingValue;
    }
}

[System.Serializable]
public class ObjectSettings
{
    public float startingValue;

    public float damageThreshold = 10f;

    public ObjectSettings(float startingValue, float damageThreshold)
    {
        this.startingValue = startingValue;
        this.damageThreshold = damageThreshold;
    }
}
