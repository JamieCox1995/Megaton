using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject gameplayUI;
    public GameObject pauseMenu;
    public GameObject optionsMenu;
    public GameObject infoScreen;
    public GameObject readyText;
    public GameObject freeCamDisplay;

    private bool wasActiveLast = false;
    private bool freeCamLastActive = false;

    private int shotsUsed = 0;
    private bool hasPickup = false;
    private bool pickupUsed = false;
    private string pickupType;

    [Header("Score UI")]
    public Text scoreText;
    private float score;        // Really ineffiencent storing 2 scores, but what ya goin do?

    [Header("Ammunition UI")]
    public Image currentAmmo;
    public Text typeText;
    public Text shotText;
    private bool pickupGainedThisShot = false;
    private int framesToWait = 5;

    public string[] ammunitions = new string[2];
    private int index = 0;

    [Header("Targeting UI")]
    public GameObject targetingImage;

    [Header("Aftershock UI")]
    public GameObject aftershockUI;
    public Slider aftershockSlider;
    public GameObject aftershockReadyImage;
    public GameObject tintImage;

    [Header("Pause Menu")]
    public GameObject toSelectOnPause;

    [Header("Info Screen")]
    public Image[] medalImages;
    public Text[] medalScores;
    public Text infoScreenText;

    [Header("Game Over UI")]
    public GameObject countdown;
    public GameObject gameOverScreen;
    public GameObject continueButton;
    public Text finalScore;
    public Text deductionDescription;
    public Text deductionText;
    public Text finalAward;
    public Image medalAchieved;
    private MedalType medal;

    [Header("Ammunition Icons")]
    public AmmunitionUIIcons currentSelectedIcons;

    [Header("Medal Icons")]
    public Sprite[] medalSprites;

    private float currentCharge = 0f;
    private float neededCharge = 0f;

    private bool countdownWasActive = false;

    private EventSystem eventSystem;

    private void RegisterEvents()
    {
        GameEventManager.instance.onScoreUpdated += OnScoreUpdated;
        GameEventManager.instance.onMedalAchieved += OnMedalAchieved;
        GameEventManager.instance.onCalculatedScore += SetUpInfoScreen;

        GameEventManager.instance.onSwitchAmmo += UpdateAmmoUI;
        GameEventManager.instance.onPickupGained += PickupGained;

        GameEventManager.instance.onMortarFire += HideTargetingCamera;
        GameEventManager.instance.onMortarFire += OnShot;
        GameEventManager.instance.onProjectileDestroyed += ShowFreeCamDisplay;
        GameEventManager.instance.onProjectileDestroyed += OnProjectileDestoryed;
        GameEventManager.instance.onTurnEnd += ShowTargetingCamera;
        
        GameEventManager.instance.onAftershockPrimed += OnAftershockPrimed;

        GameEventManager.instance.onAftershockEntered += AftershockEntered;
        GameEventManager.instance.onAftershockExited += AftershockExited;

        GameEventManager.instance.onAftershockUsed += OnAftershockUsed;

        GameEventManager.instance.onPlayerReady += OnPlayerReady;

        GameEventManager.instance.onGamePaused += OnGamePaused;
        GameEventManager.instance.onGameUnpaused += OnGameUnpaused;

        GameEventManager.instance.onShowInfo += ShowInfoScreen;
        GameEventManager.instance.onHideInfo += HideInfoScreen;

        GameEventManager.instance.onCountdownStart += StartCountdown;
        GameEventManager.instance.onCountdownUpdated += CountdownUpdate;
        GameEventManager.instance.onGameOver += OnGameOver;

        GameEventManager.instance.onLevelEventStarted += LevelEventStarted;
        GameEventManager.instance.onLevelEventEnded += LevelEventEnded;
    }

	// Use this for initialization
	void Start ()
    {
        eventSystem = FindObjectOfType<EventSystem>();

        GetAmmunitionFromPrefs();

        RegisterEvents();

        scoreText.text = "0";
        shotText.text = "";
        aftershockUI.SetActive(false);
        gameOverScreen.SetActive(false);
        targetingImage.SetActive(false);

    }

    private void SetUpInfoScreen(CalculatedScoringEvent eventArg)
    {
        for(int i = 0; i < medalScores.Length; i++)
        {
            medalScores[i].text = eventArg.scores[i].ToString("N0");
        }
    }

    private void LevelEventStarted()
    {
        countdown.SetActive(false);
    }

    private void LevelEventEnded()
    {
        if (countdownWasActive)
        {
            countdown.SetActive(true);
        }
    }

    private void AftershockEntered()
    {
        tintImage.GetComponent<Animator>().SetTrigger("On");
    }

    private void AftershockExited()
    {
        tintImage.GetComponent<Animator>().SetTrigger("Off");
    }

    private void GetAmmunitionFromPrefs()
    {
        ammunitions[0] = PlayerProgression._instance.GetEquippedProjectile(ProjectileType.Primary);

        currentAmmo.sprite = GetAmmunitionSprite(currentSelectedIcons, ammunitions[0]);
        typeText.text = ammunitions[0];
    }

    private void PickupGained(PickupEvent pickupEvent)
    {
        tintImage.GetComponent<Animator>().SetTrigger("Pickup");

        currentAmmo.sprite = GetAmmunitionSprite(currentSelectedIcons, pickupEvent.pickupType);
        pickupType = pickupEvent.pickupType;
        typeText.text = pickupEvent.pickupType;

        shotText.text = "<color=cyan>1 Pickup Remaining</color>";

        // To stop the UI from being overwritten when a projectile is destroyed, we need to block the UI update
        // if the pickup was gained this shot
        pickupGainedThisShot = true;
        hasPickup = true;
    }

    private Sprite GetAmmunitionSprite(AmmunitionUIIcons selectFrom, string ammunition)
    {
        Sprite sprite = selectFrom.standardAmmo;

        switch (ammunition)
        {
            case "Airburst":
                sprite = selectFrom.airbustAmmo;
                break;

            case "Bouncing":
                sprite = selectFrom.bouncingAmmo;
                break;

            case "Cluster":
                sprite = selectFrom.clusterAmmo;
                break;

            case "Homing":
                sprite = selectFrom.homingAmmo;
                break;

            case "Singularity":
                sprite = selectFrom.singularityAmmo;
                break;

            case "Gravity":
                sprite = selectFrom.gravityAmmo;
                break;

            case "Nuke":
                sprite = selectFrom.nukeAmmo;
                break;

            case "Acid":
                sprite = selectFrom.acidAmmo;
                break;

            case "Implosion":
                sprite = selectFrom.implosionAmmo;
                break;

            case "Laser":
                sprite = selectFrom.laserAmmo;
                break;

            case "Kinetic":
                sprite = selectFrom.kineticAmmo;
                break;
        }

        return sprite;
    }

    private void OnPlayerReady()
    {
        readyText.SetActive(false);

        ShowTargetingCamera();
    }

    private void OnShot(MortarFireEvent fireEvent)
    {
        shotText.text = fireEvent.remainingShots.ToString() + " Shots Left";

        pickupGainedThisShot = false;

        if (fireEvent.remainingShots == 1)
        {
            shotText.text = "<color=red>1 Shot Remaining</color>";
        }
        else if (fireEvent.remainingShots == 0)
        {
            shotText.text = "<color=red>No more shots</color>";
        }

        if (!hasPickup)
        {
            shotsUsed++;
        }
        else
        {
            pickupUsed = true;
            hasPickup = false;
        }

    }

    private void UpdateAmmoUI(SwitchAmmoEvent ammoEvent)
    {
        index = ammoEvent.index;

        currentAmmo.sprite = GetAmmunitionSprite(currentSelectedIcons, ammunitions[index]);
    }

    private void OnProjectileDestoryed()
    {
        if (pickupGainedThisShot == false)
        {
            currentAmmo.sprite = GetAmmunitionSprite(currentSelectedIcons, ammunitions[0]);

            typeText.text = ammunitions[0];
        }

        pickupGainedThisShot = false;
    }

    private void HideTargetingCamera(MortarFireEvent fireEvent)
    {
        targetingImage.SetActive(false);
    }

    private void ShowFreeCamDisplay()
    {
        freeCamDisplay.SetActive(true);
    }
    
    private void ShowTargetingCamera()
    {
        targetingImage.SetActive(true);
        freeCamDisplay.SetActive(false);
    }

    private void OnScoreUpdated(ScoreUpdateEvent scoreUpdate)
    {
        score += scoreUpdate.score;

        scoreText.text = score.ToString("N0");

        infoScreenText.text = score.ToString("N0");
    }

    private void StartCountdown()
    {
        GameEventManager.instance.onCountdownStart -= StartCountdown;
    }

    private void CountdownUpdate(CountdownEvent eventArg)
    {
        countdownWasActive = true;

        float time = ((int)(eventArg.timeRemaining * 10)) / 10f;

        countdown.GetComponent<Animator>().SetFloat("TimeRemaining", time);
        countdown.GetComponent<Text>().text = time.ToString("F1");
    }

    private void OnMedalAchieved(MedalAchievedEvent medalEvent)
    {
        medal = medalEvent.medalAchieved;

        medalImages[(int)medal - 1].color = Color.white;
    }

    private void UpdateAfterShockMeter(ScoreUpdateEvent scoreUpdate)
    {
        currentCharge += scoreUpdate.score;

        aftershockSlider.value = neededCharge - currentCharge;

        if (aftershockSlider.value <= 0.0000001f)
        {
            aftershockReadyImage.GetComponent<Animator>().ResetTrigger("Not Ready");
            aftershockReadyImage.GetComponent<Animator>().SetTrigger("Ready");
        }
    }

    private void OnAftershockUsed(AftershockEvent aftershockEvent)
    {
        neededCharge = aftershockEvent.newCharge;

        currentCharge = 0f;

        aftershockSlider.maxValue = neededCharge;
        aftershockSlider.value = neededCharge;

        aftershockReadyImage.GetComponent<Animator>().ResetTrigger("Ready");
        aftershockReadyImage.GetComponent<Animator>().SetTrigger("Not Ready");
    }

    private void OnAftershockPrimed()
    {
        GameEventManager.instance.onScoreUpdated += UpdateAfterShockMeter;

        aftershockUI.SetActive(true);
    }

    private void OnGamePaused()
    {
        pauseMenu.GetComponent<Animator>().ResetTrigger("Close");
        optionsMenu.GetComponent<Animator>().ResetTrigger("Close");
        pauseMenu.GetComponent<Animator>().SetTrigger("Open");

        eventSystem.SetSelectedGameObject(toSelectOnPause);
    }

    private void OnGameUnpaused()
    {
        pauseMenu.GetComponent<Animator>().SetTrigger("Close");
        optionsMenu.GetComponent<Animator>().SetTrigger("Close");
    }

    private void ShowInfoScreen()
    {
        wasActiveLast = gameplayUI.active;
        freeCamLastActive = freeCamDisplay.active;

        gameplayUI.SetActive(false);
        freeCamDisplay.SetActive(false);

        infoScreen.GetComponent<Animator>().SetTrigger("Open");
    }

    private void HideInfoScreen()
    {
        gameplayUI.SetActive(wasActiveLast);
        freeCamDisplay.SetActive(freeCamLastActive);

        infoScreen.GetComponent<Animator>().SetTrigger("Close");
    }

    private void OnGameOver()
    {
        countdown.SetActive(false);
        gameplayUI.SetActive(false);
        gameOverScreen.SetActive(true);

        eventSystem.SetSelectedGameObject(continueButton);

        float costToLose = 0f;

        finalScore.text = score.ToString("N0") + " USD";

        // Here we are setting the Deduction Text and value fields;
        deductionText.text = deductionDescription.text = "";
        deductionDescription.text += "Standard Shots x" + shotsUsed + ": ";

        // Calculate the cost of the projectiles
        float standardCost = PlayerPrefs.GetFloat("Primary Cost") * shotsUsed * 0.5f;

        deductionText.text += string.Format("<color=red>-{0, 12:N0} USD</color>", standardCost);

        costToLose += standardCost;

        if (pickupUsed)
        {
            float pickupCost = PlayerPrefs.GetFloat("Pickup Cost") * 0.25f;

            costToLose += pickupCost; 

            deductionDescription.text += "\nPickup Used:";
            deductionText.text += string.Format("\n<color=red>-{0, 12:N0} USD</color>", pickupCost);
        }

        float scoreToAward = score - costToLose;

        finalAward.text = scoreToAward.ToString("N0") + " USD";

        score = scoreToAward;

        medalAchieved.sprite = medalSprites[(int)medal];
    }
}

[System.Serializable]
public class AmmunitionUIIcons
{
    public Sprite standardAmmo;
    public Sprite acidAmmo;
    public Sprite airbustAmmo;
    public Sprite implosionAmmo;
    public Sprite clusterAmmo;
    public Sprite bouncingAmmo;
    public Sprite homingAmmo;
    public Sprite laserAmmo;
    public Sprite singularityAmmo;
    public Sprite gravityAmmo;
    public Sprite nukeAmmo;
    public Sprite kineticAmmo;
}
