using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance;

    private const string PlayerPrefKey = "Tutorial Active";

    [Header("Tutorial Behaviours")]
    public TutorialStepList TutorialSteps;
    [Header("Visual Properties")]
    public CanvasGroup TutorialUI;
    public GameObject ButtonPrefab;
    public string ControlInputFormat;
    public string GameConceptFormat;
    public Font Font;
    public int FontSize = 11;
    public Color TextColor = Color.white;

    private TutorialStep _currentStep;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        if (!GetTutorialState())
        {
            this.enabled = false;
            this.TutorialUI.Hide();
        }
        else
        {
            TutorialUILayoutUtils.SetButtonPrefab(this.ButtonPrefab);
            TutorialUILayoutUtils.SetControlInputFormat(this.ControlInputFormat);
            TutorialUILayoutUtils.SetFont(this.Font);
            TutorialUILayoutUtils.SetFontSize(this.FontSize);
            TutorialUILayoutUtils.SetColor(this.TextColor);

            GameEventManager.instance.onPlayerReady += StartTutorial;
            GameEventManager.instance.onGamePaused += HideTutorial;
            GameEventManager.instance.onGameUnpaused += ShowTutorial;
        }
    }

    IEnumerator DoTutorial()
    {
        Coroutine<TutorialStep.Result> coroutine;

        foreach (TutorialStep step in this.TutorialSteps)
        {
            _currentStep = step;

            coroutine = new Coroutine<TutorialStep.Result>(step.Run(this, this.TutorialUI));

            Debug.LogFormat("{0} started.", step.gameObject.name);

            yield return coroutine.Start(this);

            Debug.LogFormat("{0} completed.", step.gameObject.name);

            if (coroutine.Value == TutorialStep.Result.Break)
            {
                Debug.Log("Tutorial step exited early.");
                break;
            }
        }

        SetTutorialInactive();
        this.TutorialUI.Hide();

        GameEventManager.instance.onPlayerReady -= StartTutorial;
        GameEventManager.instance.onGamePaused -= HideTutorial;
        GameEventManager.instance.onGameUnpaused -= ShowTutorial;
    }

    private void StartTutorial()
    {
        StartCoroutine(DoTutorial());
    }

    private void HideTutorial()
    {
        this.TutorialUI.Hide();
    }

    private void ShowTutorial()
    {
        this.TutorialUI.Show();
    }

    public static bool IsTutorialActive()
    {
        if (Instance == null) return false;

        return GetTutorialState();
    }

    private void SetTutorialInactive()
    {
        Debug.Log("Tutorial set inactive.");
        SetTutorialState(false);
    }

    public static bool RequiresCursor()
    {
        return IsTutorialActive() && Instance._currentStep != null && Instance._currentStep.RequiresCursorControl;
    }

    [ContextMenu("Reset Tutorial State")]
    private void ResetTutorialState()
    {
        Debug.Log("Tutorial set active.");
        SetTutorialState(true);
    }

    private static void SetTutorialState(bool value)
    {
        SettingsManager._instance.gameSettings.tutorialActive = value;
        SettingsManager.Save(SettingsManager._instance.gameSettings);
    }

    private static bool GetTutorialState()
    {
        return SettingsManager._instance.gameSettings.tutorialActive;
    }
}

