using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFireMortarTutorialStep : ActionTutorialStep
{
    private Mortar _mortarInstance;

    public float LaunchElevation;
    public float LaunchRotation;
    public float LaunchForce;

    public float LaunchTime;

    public float LaunchDelay;

    public override void Setup()
    {
        base.Setup();

        _mortarInstance = FindObjectOfType<Mortar>();
    }

    public override void Cleanup()
    {
        base.Cleanup();

        _mortarInstance = null;
    }

    public override IEnumerator PerformAction()
    {
        float initialElevation = _mortarInstance.GetElevation();
        float initialRotation = _mortarInstance.GetRotation();
        float initialForce = _mortarInstance.GetForce();

        float currentTime = 0f;
        float t = 0f;

        AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

        while (currentTime < this.LaunchTime)
        {
            currentTime += Time.deltaTime;
            t = currentTime / this.LaunchTime;

            t = easing.Evaluate(t);

            float currentElevation = Mathf.LerpAngle(initialElevation, this.LaunchElevation, t);
            float currentRotation = Mathf.LerpAngle(initialRotation, this.LaunchRotation, t);
            float currentForce = Mathf.Lerp(initialForce, this.LaunchForce, t);

            _mortarInstance.SetElevation(currentElevation);
            _mortarInstance.SetRotation(currentRotation);
            _mortarInstance.SetForce(currentForce);

            yield return null;
        }

        yield return new WaitForSeconds(this.LaunchDelay);

        _mortarInstance.LaunchProjectile();
    }
}
