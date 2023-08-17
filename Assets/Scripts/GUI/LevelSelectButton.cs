using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour
{
    public string levelToLoad = "";
    public string nameToDisplay;
    public Sprite levelImage;

    public LevelSelect levelSelect;
    public GameObject thingyForSceneLoader;

    public void SetSelectorInfo()
    {
        // This is used to update the info panel
        levelSelect.ShowLevelStats(levelToLoad, nameToDisplay);
    }

    public void StartLoadLevel()
    {
        // For the level loading process, we want to instantiate the thingy and set it's info
        LevelInfo info = Instantiate(thingyForSceneLoader).GetComponent<LevelInfo>();

        info.levelName = levelToLoad;
        info.visualName = nameToDisplay;

        info.levelLoadingImage = levelImage;

        // We then want to load the loading scene
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // We are assuming that the Loading Scene will always be the second index in the build list
        SceneManager.LoadScene(1);

    }
}
