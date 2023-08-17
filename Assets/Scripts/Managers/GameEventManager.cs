using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager instance;

    public delegate void PlayerReadyDelegate();
    public event PlayerReadyDelegate onPlayerReady;

    public delegate void PlayerUnreadyDelegate();
    public event PlayerUnreadyDelegate onPlayerUnready;

    public delegate void GamePausedDelegate();
    public event GamePausedDelegate onGamePaused;

    public delegate void GameUnpausedDelegate();
    public event GameUnpausedDelegate onGameUnpaused;

    public delegate void ProjectileDestroyDelegate();
    public event ProjectileDestroyDelegate onProjectileDestroyed;

    public delegate void AftershockPrimedDelegate();
    public event AftershockPrimedDelegate onAftershockPrimed;

    public delegate void AftershockEntered();
    public event AftershockEntered onAftershockEntered;

    public delegate void AftershockExited();
    public event AftershockExited onAftershockExited;

    public delegate void EndTurnPlayerDelegate();
    public event EndTurnPlayerDelegate onTurnEnd;

    public delegate void SettingsChangedDelegate();
    public event SettingsChangedDelegate onSettingsChanged;

    public delegate void CountdownStarted();
    public event CountdownStarted onCountdownStart;

    public event Action<CountdownEvent> onCountdownUpdated;

    public delegate void ShowInfoDelegate();
    public event ShowInfoDelegate onShowInfo;

    public delegate void HideInfoDelegate();
    public event HideInfoDelegate onHideInfo;

    public delegate void GameOverDelegate();
    public event GameOverDelegate onGameOver;

    public delegate void GenericLevelEvent();

    public event GenericLevelEvent onLevelEventStarted;
    public event GenericLevelEvent onLevelEventEnded;

    public event Action<CalculatedScoringEvent> onCalculatedScore;

    public event Action<SwitchAmmoEvent> onSwitchAmmo;

    public event Action<MortarFireEvent> onMortarFire;

    public event Action<PickupEvent> onPickupGained;

    public event Action<ExplosionEvent> onExplosionEvent;

    public event Action<AftershockEvent> onAftershockUsed;

    public event Action<ScoreUpdateEvent> onScoreUpdated;

    public event Action<MedalAchievedEvent> onMedalAchieved;

    public event Action<StatAchievedEvent> onStatAchieved;

    public event Action<TimeScaleAdjustmentEvent> onStartTimeScaleAdjustment;

    public delegate void RemoveTimeScaleEventDelegate();
    public event RemoveTimeScaleEventDelegate onEndTimeScaleAdjustment;

    public event Action<ObjectDamagedEvent> onObjectDamaged;

    public delegate void CancelCoroutinesRequestedDelegate();
    public event CancelCoroutinesRequestedDelegate onCancelCoroutinesRequested;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Used to trigger the appropriate event type based on the data passed into the method
    /// </summary>
    /// <param name="eventArg"> Struct Containing the type of Event we want to trigger</param>
    public void TriggerEvent(GameEvent eventArg)
    {
        switch (eventArg.eventType)
        {
            case GameEventType.OnPlayerReady:
                TriggerPlayerReady();
                break;

            case GameEventType.OnPlayerUnready:
                TriggerPlayerUnready();
                break;

            case GameEventType.OnGamePaused:
                TriggerGamePaused();
                break;

            case GameEventType.OnGameUnpaused:
                TriggerGameUnpaused();
                break;

            case GameEventType.OnAmmoChanged:
                TriggerSwitchAmmo((SwitchAmmoEvent)eventArg);
                    break;

            case GameEventType.OnMortarFired:
                TriggerMortarFired((MortarFireEvent)eventArg);
                break;

            case GameEventType.OnExplosion:
                TriggerExplosion((ExplosionEvent)eventArg);
                break;

            case GameEventType.OnCalculatedScore:
                TriggerScoringCalculated((CalculatedScoringEvent)eventArg);
                break;

            case GameEventType.OnScoreUpdated:
                TriggerScoreUpdated((ScoreUpdateEvent)eventArg);
                break;

            case GameEventType.onMedalAchievent:
                TriggerMedalAchieved((MedalAchievedEvent)eventArg);
                break;

            case GameEventType.OnProjectileDestroyed:
                TriggerProjectileDestroyed();
                break;

            case GameEventType.OnAftershockPrimed:
                TriggerAftershockPrimed();
                break;

            case GameEventType.OnAftershockEntered:
                TriggerAftershockEntered();
                break;

            case GameEventType.OnAftershockExited:
                TriggerAftershockExited();
                break;

            case GameEventType.OnAftershockUsed:
                TriggerAftershockUsed((AftershockEvent)eventArg);
                break;

            case GameEventType.OnPickupGained:
                TriggerPickupEvent((PickupEvent)eventArg);
                break;

            case GameEventType.OnEndTurn:
                TriggerEndTurn();
                break;

            case GameEventType.OnCountdownStart:
                TriggerCountdownStart();
                break;

            case GameEventType.OnCountdownUpdated:
                TriggerCountdownUpdate((CountdownEvent)eventArg);
                break;

            case GameEventType.OnShowInfo:
                TriggerShowInfo();
                break;

            case GameEventType.OnHideInfo:
                TriggerHideInfo();
                break;

            case GameEventType.OnLevelEventStarted:
                TriggerLevelEventStart();
                break;

            case GameEventType.OnLevelEventEnded:
                TriggerLevelEventEnd();
                break;

            case GameEventType.OnGameOver:
                TriggerGameOver();
                break;

            case GameEventType.OnStartTimeScaleAdjustment:
                TriggerOnStartTimeScaleAdjustment((TimeScaleAdjustmentEvent)eventArg);
                break;

            case GameEventType.OnEndTimeScaleAdjustment:
                TriggerOnRemoveTimeScaleEvent();
                break;

            case GameEventType.OnObjectDamaged:
                TriggerOnObjectDamaged((ObjectDamagedEvent)eventArg);
                break;

            case GameEventType.OnSettingsChanged:
                TriggerSettingsChanged();
                break;

            case GameEventType.OnCancelCoroutinesRequested:
                TriggerCancelCoroutinesRequested();
                break;

            case GameEventType.OnStatAchieved:
                TriggerStatAchieved((StatAchievedEvent)eventArg);
                break;
        }
    }

    private void TriggerPlayerReady()
    {
        if (onPlayerReady != null)
        {
            onPlayerReady();
        }
    }

    private void TriggerPlayerUnready()
    {
        if (onPlayerUnready != null)
        {
            onPlayerUnready();
        }
    }

    private void TriggerGamePaused()
    {
        if (onGamePaused != null)
        {
            onGamePaused();
        }
    }

    private void TriggerGameUnpaused()
    {
        if (onGameUnpaused != null)
        {
            onGameUnpaused();
        }
    }

    private void TriggerStatAchieved(StatAchievedEvent statEvent)
    {
        if (onStatAchieved != null)
        {
            onStatAchieved(statEvent);
        }
    }

    private void TriggerSwitchAmmo(SwitchAmmoEvent ammoEvent)
    {
        if (onSwitchAmmo != null)
        {
            onSwitchAmmo(ammoEvent);
        }
    }

    private void TriggerMortarFired(MortarFireEvent fireEvent)
    {
        if (onMortarFire != null)
        {
            onMortarFire(fireEvent);
        }
    }

    private void TriggerExplosion(ExplosionEvent explosionEvent)
    {
        if(onExplosionEvent != null)
        {
            onExplosionEvent(explosionEvent);
        }
    }

    private void TriggerScoringCalculated(CalculatedScoringEvent scoreEvent)
    {
        if (onCalculatedScore != null)
        {
            onCalculatedScore(scoreEvent);
        }
    }

    private void TriggerScoreUpdated(ScoreUpdateEvent scoreEvent)
    {
        if (onScoreUpdated != null)
        {
            onScoreUpdated(scoreEvent);
        }
    }

    private void TriggerMedalAchieved(MedalAchievedEvent medalEvent)
    {
        if (onMedalAchieved != null)
        {
            onMedalAchieved(medalEvent);
        }
    }

    private void TriggerProjectileDestroyed()
    {
        if (onProjectileDestroyed != null)
        {
            onProjectileDestroyed();
        }
    }

    private void TriggerAftershockPrimed()
    {
        if (onAftershockPrimed != null)
        {
            onAftershockPrimed();
        }
    }

    private void TriggerAftershockEntered()
    {
        if(onAftershockEntered != null)
        {
            onAftershockEntered();
        }
    }

    private void TriggerAftershockExited()
    {
        if (onAftershockExited != null)
        {
            onAftershockExited();
        }
    } 

    private void TriggerAftershockUsed(AftershockEvent aftershockEvent)
    {
        if (onAftershockUsed != null)
        {
            onAftershockUsed(aftershockEvent);
        }
    }

    private void TriggerPickupEvent(PickupEvent pickUpEvent)
    {
        if (onPickupGained != null)
        {
            onPickupGained(pickUpEvent);
        }
    }

    private void TriggerEndTurn()
    {
        if (onTurnEnd != null)
        {
            onTurnEnd();
        }
    }

    private void TriggerCountdownStart()
    {
        if (onCountdownStart != null)
        {
            onCountdownStart();
        }
    }

    private void TriggerCountdownUpdate(CountdownEvent eventArg)
    {
        if (onCountdownUpdated != null)
        {
            onCountdownUpdated(eventArg);
        }
    }

    private void TriggerShowInfo()
    {
        if (onShowInfo != null)
        {
            onShowInfo();
        }
    } 

    private void TriggerHideInfo()
    {
        if(onHideInfo != null)
        {
            onHideInfo();
        }
    }

    private void TriggerLevelEventStart()
    {
        if (onLevelEventStarted != null)
        {
            onLevelEventStarted();
        }
    }

    private void TriggerLevelEventEnd()
    {
        if (onLevelEventEnded != null)
        {
            onLevelEventEnded();
        }
    }

    private void TriggerGameOver()
    {
        if (onGameOver != null)
        {
            onGameOver();
        }
    }

    private void TriggerOnStartTimeScaleAdjustment(TimeScaleAdjustmentEvent eventArg)
    {
        if (onStartTimeScaleAdjustment != null)
        {
            onStartTimeScaleAdjustment(eventArg);
        }
    }

    private void TriggerOnRemoveTimeScaleEvent()
    {
        if (onEndTimeScaleAdjustment != null)
        {
            onEndTimeScaleAdjustment();
        }
    }

    private void TriggerSettingsChanged()
    {
        if (onSettingsChanged != null)
        {
            onSettingsChanged();
        }
    }
    
    
    private void TriggerOnObjectDamaged(ObjectDamagedEvent eventArg)
    {
        if (onObjectDamaged != null)
        {
            onObjectDamaged(eventArg);
        }
    }

    private void TriggerCancelCoroutinesRequested()
    {
        if (onCancelCoroutinesRequested != null)
        {
            onCancelCoroutinesRequested();
        }
    }
}

/// <summary>
/// This class has to be extended for all of the events which will be fired in the game.
/// </summary>
public class GameEvent
{
    public GameEventType eventType;

    public GameEvent(GameEventType type)
    {
        eventType = type;
    }
}

public class SwitchAmmoEvent : GameEvent
{
    public int index;

    public SwitchAmmoEvent(GameEventType type, int i) : base (type)
    {
        index = i;
    }
}

/// <summary>
/// This event data is used when the mortar is fired.
/// </summary>
public class MortarFireEvent : GameEvent
{
    public GameObject projectile;
    public int remainingShots;

    public MortarFireEvent(GameEventType type, GameObject proj, int shots) : base (type)
    {
        projectile = proj;
        remainingShots = shots;
    }
}

public class ExplosionEvent : GameEvent
{
    public Vector3 position;
    public float size;

    public ExplosionEvent(GameEventType type, Vector3 p, float s) : base(type)
    {
        position = p;
        size = s;
    }
}

public class AftershockEvent : GameEvent
{
    public float newCharge;

    public AftershockEvent(GameEventType type, float charge) : base(type)
    {
        newCharge = charge;
    }
}

public class CalculatedScoringEvent : GameEvent
{
    public float[] scores;

    public CalculatedScoringEvent(GameEventType type, float[] s) : base(type)
    {
        scores = s;
    }
}


public class ScoreUpdateEvent : GameEvent
{
    public float score;

    public ScoreUpdateEvent(GameEventType type, float value) : base (type)
    {
        score = value;
    }
}

public class PickupEvent : GameEvent
{
    public string pickupType;

    public PickupEvent(GameEventType type, string pType) : base (type)
    {
        pickupType = pType;
    }
}

public class MedalAchievedEvent : GameEvent
{
    public float nextMedal;
    public MedalType medalAchieved;

    public MedalAchievedEvent(GameEventType type, float score, MedalType medal) : base(type)
    {
        nextMedal = score;
        medalAchieved = medal;
    }
}

public class StatAchievedEvent : GameEvent
{
    public int id;

    public StatAchievedEvent(GameEventType type, int id) : base(type)
    {
        this.id = id;
    }
}

public class TimeScaleAdjustmentEvent : GameEvent
{
    public Settings settings;

    public TimeScaleAdjustmentEvent(GameEventType type, float timeScale) : base(type)
    {
        this.settings = new Settings(timeScale);
    }

    public TimeScaleAdjustmentEvent(GameEventType type, Settings settings) : base(type)
    {
        this.settings = settings;
    }

    public class Settings
    {
        public float timeScale;
        public bool hasTransition;
        public AnimationCurve easing;
        public float transitionTime;

        public Settings(float timeScale)
        {
            if (timeScale < 0f) throw new ArgumentOutOfRangeException("timeScale");

            this.timeScale = timeScale;
            this.hasTransition = false;
            this.easing = null;
            this.transitionTime = 0f;
        }

        public Settings(float timeScale, AnimationCurve easing, float transitionTime)
        {
            if (timeScale < 0f) throw new ArgumentOutOfRangeException("timeScale");
            if (transitionTime < 0f) throw new ArgumentOutOfRangeException("transitionTime");
            if (easing == null) throw new ArgumentNullException("easing");

            this.timeScale = timeScale;
            this.hasTransition = true;
            this.easing = easing;
            this.transitionTime = transitionTime;
        }
    }
}

public class CountdownEvent : GameEvent
{
    public float timeRemaining;

    public CountdownEvent(float timeRemaining) : base(GameEventType.OnCountdownUpdated)
    {
        this.timeRemaining = timeRemaining;
    }
}

public class ObjectDamagedEvent : GameEvent
{
    public FracturedObject target;

    public ObjectDamagedEvent(FracturedObject target) : base(GameEventType.OnObjectDamaged)
    {
        this.target = target;
    }
}

public enum GameEventType
{
    OnPlayerReady,
    OnPlayerUnready,
    OnAmmoChanged,
    OnMortarFired,
    OnExplosion,
    OnCalculatedScore,
    OnScoreUpdated,
    onMedalAchievent,
    OnProjectileDestroyed,
    OnAftershockPrimed,
    OnAftershockEntered,
    OnAftershockExited,
    OnAftershockUsed,
    OnPickupGained,
    OnEndTurn,
    OnGamePaused,
    OnGameUnpaused,
    OnCountdownStart,
    OnCountdownUpdated,
    OnShowInfo,
    OnHideInfo,
    OnLevelEventStarted,
    OnLevelEventEnded,
    OnGameOver,
    OnStartTimeScaleAdjustment,
    OnEndTimeScaleAdjustment,
    OnObjectDamaged,
    OnSettingsChanged,
    OnStatAchieved,
    OnCancelCoroutinesRequested,
}