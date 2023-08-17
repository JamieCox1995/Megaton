using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TutorialStepList : MonoBehaviour, IEnumerable<TutorialStep>
{
    [SerializeField, HideInInspector]
    private List<TutorialStep> _steps;

    public IEnumerator<TutorialStep> GetEnumerator()
    {
        if (_steps == null) throw new InvalidOperationException();

        foreach (TutorialStep step in _steps) yield return step;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

#if UNITY_EDITOR
    void Update () {
        if (Application.isPlaying) return;
        
        _steps = new List<TutorialStep>();

        foreach (Transform child in transform)
        {
            TutorialStep step = child.GetComponent<TutorialStep>();

            if (step != null) _steps.Add(step);
        }
	}
#endif
}
