using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectComponent : MonoBehaviour
{
    public string menuAnimatorName = "Menu Canvases";

    // We're storing the map results.
    private MapResults mapResults;
    private string levelName;

	public void OnClick()
    {
        GameObject.Find(menuAnimatorName).GetComponent<Animator>().SetTrigger("Level Selected");
        FindObjectOfType<RegionSelect>().SetHighscoreDisplay(mapResults, levelName);
    }

    public void SetMapResults(string levelName)
    {
        mapResults = PlayerProgression._instance.GetMapResults(levelName);
        this.levelName = levelName;
    }
}
