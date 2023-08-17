using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorPhysics : ScriptableWizard
{
    public float TimeStep;
    public float TimeToSimulate;

    public bool CloseOnComplete;

    private IEnumerator coroutine;
    private Rigidbody[] selectedRigidbodies;
    private WizardState state;

    private static float lastTimeToSimulate = 1f;
    private static bool lastCloseOnComplete;

    [MenuItem("Window/Editor Physics")]
    private static void Initialize()
    {
        var wizard = ScriptableWizard.DisplayWizard<EditorPhysics>("Editor Physics", "Apply", "Reset Velocities");
        wizard.TimeStep = Time.fixedDeltaTime;
        wizard.TimeToSimulate = lastTimeToSimulate;
        wizard.CloseOnComplete = lastCloseOnComplete;
        wizard.Show();
    }

    private void OnGUI()
    {
        var objs = Selection.objects;

        if (state != WizardState.Active)
        {
            helpString = string.Format("{0} object{1} selected", objs.Length, objs.Length == 1 ? "" : "s");
        }

        EditorGUILayout.HelpBox(helpString, MessageType.None);

        this.TimeStep = EditorGUILayout.FloatField("Time Step", this.TimeStep);
        this.TimeToSimulate = EditorGUILayout.FloatField("Time To Simulate", this.TimeToSimulate);
        lastTimeToSimulate = this.TimeToSimulate;
        this.CloseOnComplete = GUILayout.Toggle(this.CloseOnComplete, "Close On Complete?");
        lastCloseOnComplete = this.CloseOnComplete;
        
        GUILayout.BeginHorizontal();
        GUI.enabled = state == WizardState.Idle;
        if (GUILayout.Button("Apply"))
        {
            OnWizardCreate();
        }

        if (GUILayout.Button("Reset Velocities"))
        {
            OnWizardOtherButton();
        }
        GUI.enabled = true;

        Repaint();

        GUILayout.EndHorizontal();
    }

    private void OnWizardCreate()
    {
        selectedRigidbodies = Selection.GetFiltered<Rigidbody>(SelectionMode.Unfiltered);

        EditorApplication.update += EditorUpdate;
    }

    private void OnWizardOtherButton()
    {
        selectedRigidbodies = Selection.GetFiltered<Rigidbody>(SelectionMode.Unfiltered);

        foreach (var rigidbody in selectedRigidbodies)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void EditorUpdate()
    {
        state = WizardState.Active;
        if (coroutine == null) coroutine = SimulatePhysics(this.TimeStep, this.TimeToSimulate, selectedRigidbodies);

        if (!coroutine.MoveNext())
        {
            EditorApplication.update -= EditorUpdate;
            state = WizardState.Idle;
            coroutine = null;
            if (this.CloseOnComplete) Close();
        }
    }

    private IEnumerator SimulatePhysics(float step, float totalTime, Rigidbody[] rigidbodies)
    {
        var transforms = rigidbodies.Select(rb => rb.transform).ToArray();
        Undo.RegisterCompleteObjectUndo(transforms, string.Format("Editor Physics ({0} sec)", totalTime));

        float timer = 0f;
        float frameTimer = 0f;
        Physics.autoSimulation = false;

        Dictionary<Rigidbody, bool> kinematicState = SetSceneRigidbodiesKinematic(except: rigidbodies);

        while (timer < totalTime)
        {
            timer += Time.deltaTime;
            frameTimer += Time.deltaTime;

            while (frameTimer >= step)
            {
                frameTimer -= step;
                Physics.Simulate(step);
            }

            helpString = string.Format("Simulating - {0:F2}% ({1:F2}s of {2:F2}s)", 100f * timer / totalTime, timer, totalTime);

            yield return new WaitForEndOfFrame();
        }

        Physics.autoSimulation = true;
        Physics.SyncTransforms();

        ResetSceneRigidbodies(kinematicState);

        yield return new WaitForEndOfFrame();

        Undo.FlushUndoRecordObjects();
    }

    private static Dictionary<Rigidbody, bool> SetSceneRigidbodiesKinematic(Rigidbody[] except)
    {
        var allRigidbodies = FindObjectsOfType<Rigidbody>();

        Dictionary<Rigidbody, bool> kinematicState = new Dictionary<Rigidbody, bool>();

        foreach (var sceneRb in allRigidbodies.Except(except))
        {
            kinematicState.Add(sceneRb, sceneRb.isKinematic);
            sceneRb.isKinematic = true;
        }

        return kinematicState;
    }

    private static void ResetSceneRigidbodies(Dictionary<Rigidbody, bool> kinematicState)
    {
        foreach (var kvp in kinematicState)
        {
            kvp.Key.isKinematic = kvp.Value;
        }
    }

    private enum WizardState
    {
        Idle,
        Active
    }
}
