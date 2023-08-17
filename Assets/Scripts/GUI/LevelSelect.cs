using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public GameObject statsObject;
    public Animator statsAnim;

    [Header("GUI Objects")]
    public GameObject awardedStatsGroup;
    public Text level;
    public Text highscore;
    public Image[] medals;
    public Text noStatsText;

    public void ShowLevelStats(string levelName, string visualName)
    {
        statsAnim.SetTrigger("Show");

        MapResults mapResult = PlayerProgression._instance.savedProgression.mapResults.SingleOrDefault(l => l.levelName == levelName);

        level.text = visualName;

        if (mapResult != null)
        {
            noStatsText.gameObject.SetActive(false);
            awardedStatsGroup.SetActive(true);

            // Here we want to go through and set the stats for the level
            highscore.text = mapResult.highestScore.ToString("N0");

            for(int i = 0; i < mapResult.bestMedalAchieved + 1; i++)
            {
                medals[i].color = Color.white;
            }
        }
        else
        {
            // Hide the high score and the medals and display a text that says "No Recorded Stats"
            noStatsText.gameObject.SetActive(true);
            awardedStatsGroup.SetActive(false);
        }
    }

    public void HideStats()
    {
        statsAnim.SetTrigger("Hide");
    }
}
