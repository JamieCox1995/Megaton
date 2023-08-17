using System.IO;
using System.Linq;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UnlockManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private Text projectileName;
    [SerializeField]
    private Text projectileDescription;
    [SerializeField]
    private Text projectileRequirements;
    [SerializeField]
    private Text playerScore;

    [SerializeField]
    private VideoPlayer projectilePreviewer;


    [Header("Projectile UI Buttons")]
    public UnlockButton[] primaryButtons;
    public UnlockButton[] pickupButtons;

    [Header("Projectile Database")]
    public ProjectileDatabase projectileDatabase;

    private string currentPrimary;
    private string currentPickup;
    private decimal playerMoney;

    private List<string> unlockedProjectiles = new List<string>();

    [SerializeField]
    private bool debug;

    private void Start()
    {
        unlockedProjectiles = PlayerProgression._instance.savedProgression.unlockedProjectiles;

        ValidatePlayerPrefs();

        UpdateButtons();

        playerScore.text = "Funds: " + playerMoney.ToString("N0");

        SetProjectileCosts();

        projectilePreviewer.clip = null;
        projectilePreviewer.targetTexture.Release();
    }

    private void SetProjectileCosts()
    {
        float pCost, sCost;

        pCost = projectileDatabase.primaryStats.First(x => x.name == currentPrimary).cost;
        sCost = projectileDatabase.pickupStats.First(x => x.name == currentPickup).cost;

        PlayerPrefs.SetFloat("Primary Cost", pCost);
        PlayerPrefs.SetFloat("Pickup Cost", sCost);
    }

    private void ValidatePlayerPrefs()
    {
        currentPrimary = PlayerProgression._instance.GetEquippedProjectile(ProjectileType.Primary);
        currentPickup = PlayerProgression._instance.GetEquippedProjectile(ProjectileType.Pickup);

        playerMoney = PlayerProgression._instance.GetScore();
    } 

    private void UpdateButtons()
    {
        // We want to iterate through all of the buttons to see if the player has unlocked the projectile it represents
        for (int index = 0; index < primaryButtons.Length; index++)
        {
            primaryButtons[index].stats = projectileDatabase.primaryStats[index];

            SetButtonMode(primaryButtons[index]);

            primaryButtons[index].costText.text = "Cost: " + projectileDatabase.primaryStats[index].cost.ToString("N0") + "USD";    // Using USD so we can't using fucking EM
        }

        for (int index = 0; index < pickupButtons.Length; index++)
        {
            pickupButtons[index].stats = projectileDatabase.pickupStats[index];

            SetButtonMode(pickupButtons[index]);

            pickupButtons[index].costText.text = "Cost: " + projectileDatabase.pickupStats[index].cost.ToString("N0") + "USD";    // Using USD so we can't using fucking EM
        }
    }

    private void SetButtonMode(UnlockButton button)
    {
        // Here we want to set the buttons state based on whether the player has the correct funds and requirements have been met

        // The first check we do to see if the projectile has been equipped
        if (button.stats.name == PlayerProgression._instance.savedProgression.primaryProjectile || button.stats.name == PlayerProgression._instance.savedProgression.pickupProjectile)
        {
            button.SetState(UnlockButton.ButtonState.Active);
            return;
        }

        // Now we are checking to see if the player has bought the projectile  but it has not been equipped
        if (PlayerProgression._instance.savedProgression.unlockedProjectiles.Contains(button.stats.name))
        {
            button.SetState(UnlockButton.ButtonState.Equippable);
            return;
        }

        // We want to check to see if the player has met the requirements for the projectile
        if (button.stats.eventRequirements.Length != 0)         // Basic sanity check to make sure we don't spaz out on a projectile that has 0 requirements
        {
            // Getting the players saved completed events
            List<ProgressionStat> completed = PlayerProgression._instance.savedProgression.completedEvents;

            // Now we want to iterate over each requirement
            foreach (ProgressionStat requirement in button.stats.eventRequirements)
            {
                if (completed.FirstOrDefault(req => req.id == requirement.id) == null)  // FirstOrDefault will return null if the requirement hasn't been found. We shall use this to return when 1 requirement has not been met
                {
                    button.SetState(UnlockButton.ButtonState.Locked);

                    return;
                }
            }
        }

        // Now that we know the player has met the requirements, we shall set the buttons state to purchasable
        button.SetState(UnlockButton.ButtonState.Purchasable);
    }

    public void UpdateProjectileInformation(string name, string desc, VideoClip clip, ProgressionStat[] requirements)
    {
        projectileName.text = name + " Projectile";
        projectileDescription.text = desc;

        // Setting the requirements
        projectileRequirements.text = "";
        
        if (requirements.Length == 0 )
        {
            projectileRequirements.text = "- No Requirements";
        }


        foreach(ProgressionStat require in requirements)
        {
            // Get the text from the ProgressionStatManager
            string text = "- " + ProgressionStatManager._instance.GetDescription(require.id) + "\n";
            projectileRequirements.text += text;
        }

        // Set the video to play.
        projectilePreviewer.clip = clip;
        projectilePreviewer.Play();

    }

    public void UpdateProjectile(ProjectileStats stats, UnlockButton.ButtonState buttonState)
    {
        if (CheckForStatsName(projectileDatabase.primaryStats, stats))
        {
            if (SetProjectileUnlocked(projectileDatabase.primaryStats, stats, buttonState))
            {
                stats.unlocked = true;

                bool isEquippedOrActiveAndUnlocked = PlayerProgression._instance.savedProgression.unlockedProjectiles.Contains(stats.name) || buttonState == UnlockButton.ButtonState.Active || buttonState == UnlockButton.ButtonState.Equippable;

                if (isEquippedOrActiveAndUnlocked == false)
                {
                    PlayerProgression._instance.AddNewUnlock(stats.name, stats.cost);

                    playerMoney -= (decimal)stats.cost;
                    playerScore.text = "Funds: " + playerMoney.ToString("N0");
                }

                PlayerProgression._instance.SetEquippedProjectile(ProjectileType.Primary, stats.name);

                SetProjectileCosts();
            }
        }
        else
        {
            if (SetProjectileUnlocked(projectileDatabase.pickupStats, stats, buttonState))
            {
                stats.unlocked = true;

                bool isEquippedOrActiveAndUnlocked = PlayerProgression._instance.savedProgression.unlockedProjectiles.Contains(stats.name) || buttonState == UnlockButton.ButtonState.Active || buttonState == UnlockButton.ButtonState.Equippable;

                if (isEquippedOrActiveAndUnlocked == false)
                {
                    PlayerProgression._instance.AddNewUnlock(stats.name, stats.cost);

                    playerMoney -= (decimal)stats.cost;
                    playerScore.text = "Funds: " + playerMoney.ToString("N0");
                }

                PlayerProgression._instance.SetEquippedProjectile(ProjectileType.Pickup, stats.name);

                SetProjectileCosts();
            }
        }

        UpdateButtons();
    }

    private bool CheckForStatsName(ProjectileStats[] array, ProjectileStats toCheck)
    {
        foreach(ProjectileStats stat in array)
        {
            if (toCheck.name == stat.name)
            {
                return true;
            }
        }

        return false;
    }

    private bool SetProjectileUnlocked(ProjectileStats[] array, ProjectileStats toUpdate, UnlockButton.ButtonState currentState)
    {
        foreach (ProjectileStats stat in array)
        {
            if (toUpdate.name == stat.name)
            {
                if (currentState != UnlockButton.ButtonState.Locked)
                {
                    return unlockedProjectiles.Contains(stat.name) || playerMoney >= (decimal)stat.cost;
                }
            }
        }

        return false;
    }
}

[System.Serializable]
public class ProjectileDatabase
{
    public ProjectileStats[] primaryStats;
    public ProjectileStats[] pickupStats;
}

[System.Serializable]
public class ProjectileStats
{
    public string name = " ";
    public float cost = 10000;

    [TextArea(3, 5)]
    public string shortDescription;
    [TextArea(3, 8)]
    public string description = " ";

    // This is an array of strings that correspond to events which may happen in-game.
    // The PlayerProgression 'completedEvents' list must contain all of these requirements
    // for the player to be able to purchase the projectile.
    public ProgressionStat[] eventRequirements;

    public bool unlocked = false; 
}
