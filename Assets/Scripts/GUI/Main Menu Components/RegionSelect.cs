using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegionSelect : MonoBehaviour
{
    [Header("Region Data:")]
    public RegionData[] regions;

    [Header("Level Select Buttons: ")]
    public GameObject buttonPrefab;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    [Header("UI Components: ")]
    public OscilloscopeText regionNameLabel;
    public OscilloscopeText moneyText;
    public OscilloscopeText medalText;
    public OscilloscopeText unlockCost;
    public OscilloscopeText unlockFunds;

    [Header("Global Objects: ")]
    public Animator menuAnimator;
    public GameObject levelSelectPanel;
    public LevelLoader levelLoader;
    public PhosphorGraphics phosphorImage;
    private RegionData currentRegion;
    private string selectedLevelName = "";

    private void Start()
    {
        // Here we want to retrieve the player's unlocked regions, and set the RegionData's unlocked booleans.
        if (regions == null)
        {
            regions = FindObjectsOfType<RegionData>();
        }

        List<string> unlocked = PlayerProgression._instance.savedProgression.unlockedRegions;

        for(int index = 0; index < regions.Length; index++)
        {
            if (unlocked.Contains("Region " + regions[index].regionIndex.ToString()))
            {
                regions[index].unlocked = true;
            }
        }
    }

    public void OnRegionSelected(RegionData regionData)
    {
        currentRegion = regionData;

        if (!regionData.unlocked)
        {
            SetupUnlockScreen();

            // As we have not unlocked the region, we want to open the Unlock menu.
            menuAnimator.SetTrigger("Unlock Region");
        }
        else
        {
            // We want to set the menu Animator to the "Region Selected" state.
            menuAnimator.SetTrigger("Region Selected");

            SetupLevelButtons(regionData);
        }
    }

    private void SetupUnlockScreen()
    {
        regionNameLabel.Text = string.Format("{0}:", currentRegion.regionName);

        unlockCost.Text = "$" + currentRegion.regionCost.ToString("N0");
        unlockFunds.Text = "$" + PlayerProgression._instance.savedProgression.playerMoney.ToString("N0");
    }

    private void SetupLevelButtons(RegionData regionData)
    {
        ClearPreviousButtons();

        for (int index = 0; index < regionData.levels.Length; index++)
        {
            // Spawn in a new button and adding it to the spawnedButtons list
            GameObject button = Instantiate(buttonPrefab, levelSelectPanel.transform);
            spawnedButtons.Add(button);

            // Now that we have spawned the button, we will want to grab it's text component and set it's name
            OscilloscopeText text = button.GetComponentInChildren<OscilloscopeText>();
            text.PhosphorGraphics = phosphorImage;
            button.GetComponent<OscilloscopeButton>().PhosphorGraphics = phosphorImage;

            text.Text = regionData.levels[index].LevelName;

            // Now we are going to set up the map results on the button
            button.GetComponent<LevelSelectComponent>().SetMapResults(regionData.levels[index].SceneName);
        }
    }

    public void UnlockRegion()
    {
        // We want to check to see if the player has enough money to unlock the new region.
        decimal playerMoney = PlayerProgression._instance.savedProgression.playerMoney;

        if (playerMoney - currentRegion.regionCost >= 0)
        {
            // We want to add the current region index to the saved progression
            string newUnlock = "Region " + currentRegion.regionIndex;
            decimal newMoney = playerMoney - currentRegion.regionCost;

            PlayerProgression._instance.savedProgression.playerMoney = newMoney;
            PlayerProgression._instance.savedProgression.unlockedRegions.Add(newUnlock);

            PlayerProgression.SaveProgress();

            menuAnimator.SetTrigger("Region Selected");
            SetupLevelButtons(currentRegion);
        }
    }

    public void SetHighscoreDisplay(MapResults results, string levelName)
    {
        // Basically we're checking to see if the results contains data
        if (results != null)
        {
            moneyText.Text = string.Format("${0}", results.highestScore.ToString("N0"));
            medalText.Text = MedalName(results.bestMedalAchieved);
        }
        else
        {
            moneyText.Text = "N/A";
            medalText.Text = "No Medal";
        }

        selectedLevelName = levelName;
    }

    public void LoadLevel()
    {
        Debug.LogWarning("Loading Level; " + selectedLevelName);
        menuAnimator.SetTrigger("Loading");

        LevelData levelToLoad = currentRegion.levels.ToList().First(l => l.SceneName == selectedLevelName);

        levelLoader.StartLoadingProcess(levelToLoad.LevelName, levelToLoad.SceneName);
    }

    private string MedalName(int index)
    {
        string[] medals = new string[] { "No Medal", "Bronze", "Silver", "Gold", "Platinum" };

        return medals[index];
    }

    private void ClearPreviousButtons()
    {
        for(int i = 0; i < spawnedButtons.Count; i++)
        {
            Destroy(spawnedButtons[i].gameObject);
        }

        spawnedButtons = new List<GameObject>();
    }


}
