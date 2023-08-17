using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public CursorLockMode lockMode;

    [Header("Score Settings")]
    public float score;
    public float[] medalThresholds = new float[3];
    
    private MedalType currentMedal = MedalType.None;
    private Dictionary<MedalType, float> medalScores = new Dictionary<MedalType, float> { { MedalType.Bronze, 0 }, { MedalType.Silver, 0 }, { MedalType.Gold, 0 }, { MedalType.Platinum, 0 } };               // This uses the medal type as the key, and the score for the value

    private bool processing = false;

    [Header("Game Settings")]
    public float timeBeforeGameEnds = 30f;
    private float timeLeft;
    private float skipTimer = 0f;

    [SerializeField]
    private float endTurnTimer = 10f;
    private bool gamePlaying = false;
    private bool paused = false;
    private bool gameOver = false;

    private GameState currentState = GameState.Unready;
    private GameState previousState;

    private int shotsUsed = 0;
    private bool hasPickup = false;
    private bool usedPickup = false;

    private bool inLevelEvent = false;

    [Header("Audio Settings")]
    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    private AudioMixerSnapshot normalSnap;
    [SerializeField]
    private AudioMixerSnapshot pausedSnap;
    [SerializeField]
    private AudioMixerSnapshot slowSnap;
    [SerializeField]
    private float transitionTime = .1f;

    private void RegisterEvents()
    {
        GameEventManager.instance.onScoreUpdated += OnScoreUpdated;
        GameEventManager.instance.onProjectileDestroyed += OnProjectileDestroyed;
        GameEventManager.instance.onAftershockPrimed += StartEndGame;
        GameEventManager.instance.onAftershockUsed += IncreaseTimeLeft;

        GameEventManager.instance.onAftershockEntered += SlowTime;
        GameEventManager.instance.onAftershockExited += ReturnTime;

        GameEventManager.instance.onLevelEventStarted += OnLevelEventStarted;
        GameEventManager.instance.onLevelEventEnded += OnLevelEventEnded;

        GameEventManager.instance.onPickupGained += PickupGain;
        GameEventManager.instance.onMortarFire += OnShot;
    }

    private void OnLevelEventStarted()
    {
        inLevelEvent = true;
    }

    private void OnLevelEventEnded()
    {
        inLevelEvent = false;
    }

	// Use this for initialization
	void Start ()
    {
        //Application.targetFrameRate = 30;

	    if (instance == null)
        {
            instance = this;
        }

        RegisterEvents();

        CalculateScoring();

        //GameEventManager.instance.TriggerEvent(new MedalAchievedEvent(GameEventType.onMedalAchievent, medalScores[0], 0));
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Checking to see if the player has not readied up yet. If they haven't we shall wait for them to press any key.
        if (currentState == GameState.Unready)
        {
            if (ServiceLocator.Get<IInputProxyService>().anyKeyDown)
            {
                previousState = currentState;
                currentState = GameState.Targeting;

                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnPlayerReady));
            }
        }
        else
        {
            // Here we are setting the visibility of the mouse cursor depending on the current state of the game.
            // We want it hidden if the game is not paused, the game is not over, and if there is no tutorial.
            if (currentState == GameState.GameOver || Tutorial.RequiresCursor())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                if (currentState == GameState.Paused)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;
                }
                else
                {
                    Cursor.visible = false;
                    Cursor.lockState = lockMode;
                }
            }

            // If the player's turn has ended, we want to allow them to be able to skip the "FreeCam" mode if they hold down
            // a key for X number of seconds.
            if (currentState == GameState.TurnOver)
            {
                if (ServiceLocator.Get<IInputProxyService>().GetButtonDown("Launch"))
                {
                    Debug.Log("Starting Skip");
                    skipTimer = 0f;
                }

                if (ServiceLocator.Get<IInputProxyService>().GetButton("Launch"))
                {
                    skipTimer += Time.deltaTime;

                    if (skipTimer >= 3f)
                    {
                        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnEndTurn));

                        previousState = currentState;
                        currentState = GameState.Targeting;
                    }
                }

                if (ServiceLocator.Get<IInputProxyService>().GetButtonUp("Launch")) skipTimer = 0f;
            }

            if (ServiceLocator.Get<IInputProxyService>().GetButtonDown("Info"))
            {
                GameEventManager.instance.TriggerEvent(new TimeScaleAdjustmentEvent(GameEventType.OnStartTimeScaleAdjustment, 0.25f));
                // Trigger Info Shown
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnShowInfo));
            }
            else if (ServiceLocator.Get<IInputProxyService>().GetButtonUp("Info"))
            {
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnEndTimeScaleAdjustment));
                // Trigger Info Hidden
                GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnHideInfo));
            }
        }

        if (ServiceLocator.Get<IInputProxyService>().GetButtonDown("Pause"))
        {
            if (currentState == GameState.Paused)
            {
                UnpauseGame();
            }
            else
            {
                PauseGame();
            }
            return;
        }

        // Here we are detecting if the player presses any key and calling the EventManager
        /*if (ServiceLocator.Get<IInputProxyService>().anyKeyDown && gamePlaying == false)
        {
            GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnPlayerReady));
            gamePlaying = true;
        }*/

        CheckMedal();
	}

    private void CheckMedal()
    {
        MedalType newMedal = medalScores.Where(key => key.Value <= score).Max(key => key.Key);

        if (newMedal != currentMedal)
        {
            currentMedal = newMedal;

            GameEventManager.instance.TriggerEvent(new MedalAchievedEvent(GameEventType.onMedalAchievent, medalScores[currentMedal], currentMedal));
        }
    }

    private void CalculateScoring()
    {
        PhysicsObject[] objs = FindObjectsOfType<PhysicsObject>();
        AddScoreOnDamage[] objs2 = FindObjectsOfType<AddScoreOnDamage>();


        float totalScore = 0f;

        foreach (PhysicsObject obj in objs)
        {
            totalScore += obj.GetValue();
        }

        foreach(AddScoreOnDamage obj in objs2)
        {
            totalScore += obj.GetValue();
        }

        MedalType[] types = System.Enum.GetValues(typeof(MedalType)) as MedalType[];

        for(int i = 1; i < types.Length; i++)
        {
            medalScores[types[i]] = totalScore * (medalThresholds[i - 1] / 100f);
        }

        medalScores.Add(MedalType.None, 0);

        GameEventManager.instance.TriggerEvent(new CalculatedScoringEvent(GameEventType.OnCalculatedScore, medalScores.Values.ToArray()));
    }

    public void PauseGame()
    {
        if (currentState != GameState.GameOver)
        {
            GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnGamePaused));

            TimeScaleAdjustmentEvent @event = new TimeScaleAdjustmentEvent(GameEventType.OnStartTimeScaleAdjustment, 0f);
            GameEventManager.instance.TriggerEvent(@event);

            pausedSnap.TransitionTo(transitionTime);

            previousState = currentState;
            currentState = GameState.Paused;
        }
    }

    public void UnpauseGame()
    {
        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnGameUnpaused));

        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnEndTimeScaleAdjustment));

        normalSnap.TransitionTo(transitionTime);

        currentState = previousState;
    }

    private void OnScoreUpdated(ScoreUpdateEvent scoreEvent)
    {
        score += scoreEvent.score;
    }

    private void PickupGain(PickupEvent pickup)
    {
        hasPickup = true;
    }

    private void OnShot(MortarFireEvent fireEvent)
    {
        if (!hasPickup)
        {
            shotsUsed++;
        }
        else
        {
            usedPickup = true;
        }
    }

    private void OnProjectileDestroyed()
    {
        previousState = currentState;
        currentState = GameState.TurnOver;

        StartCoroutine("EndTurn");
    }

    private void OnGameOver()
    {
        previousState = currentState;
        currentState = GameState.GameOver;

        // We want to work out what the players final score is.
        float finalScore = score;
        float costToDeduct = 0f;

        costToDeduct += PlayerPrefs.GetFloat("Primary Cost") * shotsUsed * 0.5f;

        if (usedPickup) costToDeduct += PlayerPrefs.GetFloat("Pickup Cost") * 0.25f;

        finalScore -= costToDeduct;

        PlayerProgression._instance.UpdateScore(finalScore);

        PlayerProgression._instance.SetLevelResults(SceneManager.GetActiveScene().name, finalScore, (int)currentMedal);

        AchievementManager._instance.CheckMedalAchievements();
        AchievementManager._instance.CheckIfPlayerBroke();

        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnGameOver));

        pausedSnap.TransitionTo(transitionTime);
    }

    public void LeaveGame()
    {
        /*float currentMoney = PlayerPrefs.GetFloat("Player Money");

        currentMoney += score - (score * 0.4f);

        PlayerPrefs.SetFloat("Player Money", currentMoney);*/
    }

    private void StartEndGame()
    {
        StartCoroutine("EndGameTimer");
    }

    private void SlowTime()
    {
        slowSnap.TransitionTo(transitionTime);
    }

    private void ReturnTime()
    {
        normalSnap.TransitionTo(transitionTime);
    }

    private void IncreaseTimeLeft(AftershockEvent aftershockEvent)
    {
        timeLeft += 5f;

        timeLeft = Mathf.Clamp(timeLeft, 0f, 15f);
    }

    private IEnumerator SkipFreeCamera()
    {
        yield return new WaitForSecondsRealtime(5f);

        Debug.Log("Skipped Fre Camera");

        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnEndTurn));

        previousState = currentState;
        currentState = GameState.Targeting;
    }

    private IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(endTurnTimer);

        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnEndTurn));

        previousState = currentState;
        currentState = GameState.Targeting;
    }

    private IEnumerator EndGameTimer()
    {
        timeLeft = timeBeforeGameEnds;

        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnCountdownStart));

        while (timeLeft > 0)
        {
            if (!inLevelEvent)
            {
                timeLeft -= Time.deltaTime;

                timeLeft = Mathf.Max(0f, timeLeft);

                GameEventManager.instance.TriggerEvent(new CountdownEvent(timeLeft));
            }

            yield return null;
        }

        ServiceLocator.Get<IInputManager>().DisableAllAxes();
        yield return new WaitForSecondsRealtime(1f);
        ServiceLocator.Get<IInputManager>().EnableAllAxes();

        // Call Game Over.
        OnGameOver();
    }
}

public enum MedalType { None, Bronze, Silver, Gold, Platinum };

public enum GameState { Unready, Paused, Targeting, InFlight, Aftershock, TurnOver, GameOver };
