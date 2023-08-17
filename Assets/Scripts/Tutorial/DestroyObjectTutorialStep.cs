using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjectTutorialStep : EventTutorialStep
{
    public Vector3 TargetPosition;
    public float IndicatorOffset;
    public GameObject Indicator;

    private GameObject _indicatorInstance;
    private Mortar _mortarInstance;
    private int _mortarInitialCount;

    protected override void RegisterEventHandler()
    {
        GameEventManager.instance.onExplosionEvent += ExplosionEventHandler;
    }

    protected override void DeregisterEventHandler()
    {
        GameEventManager.instance.onExplosionEvent -= ExplosionEventHandler;
    }

    private void ShowActionIndicator()
    {
        Vector3 indicatorPosition = this.TargetPosition + Vector3.up * this.IndicatorOffset;
        
        _indicatorInstance = GameObject.Instantiate(this.Indicator, indicatorPosition, Quaternion.identity);
    }

    private void HideActionIndicator()
    {
        if (_indicatorInstance != null)
        {
            Destroy(_indicatorInstance);
            _indicatorInstance = null;
        }
    }

    protected override void Setup()
    {
        base.Setup();

        ShowActionIndicator();
        _mortarInstance = FindObjectOfType<Mortar>();

        _mortarInitialCount = _mortarInstance.GetNumberOfShots();
    }

    protected override void Cleanup()
    {
        base.Cleanup();

        HideActionIndicator();
        _mortarInstance = null;
    }

    private void ExplosionEventHandler(ExplosionEvent e)
    {
        float sqrDistance = (e.position - this.TargetPosition).sqrMagnitude;

        float maxSqrDistance = 20f * 20f;

        if (sqrDistance < maxSqrDistance)
        {
            MarkAsHandled();
            _mortarInstance.SetNumberOfShots(1);
        }
        else
        {
            _mortarInstance.SetNumberOfShots(_mortarInitialCount);
        }
    }
}
