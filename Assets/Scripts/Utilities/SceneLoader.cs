using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [TextArea]
    public string[] tipsToDisplay;
    private int currentlyDisplaying = 0;

    public float tipDisplayTime = 15f;

    [SerializeField]
    private int sceneID = 2;

    public Slider progressBar;
    public Text tipDisplay;
    public Text travelingTo;
    public Image levelImage;

    private LevelInfo levelInfo;
    private string sceneName = "_MainMenu"; // Here we are setting the default value to Main Menu
    private bool loadInitiated = false;

	// Use this for initialization
	void Start ()
    {
        ShuffleTips();

        InvokeRepeating("UpdateTipDisplay", 0f, tipDisplayTime);

        //StartCoroutine("LoadLevelAsync");
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (levelInfo == null)
        {
            levelInfo = FindObjectOfType<LevelInfo>();
        }
        else
        {
            if (!loadInitiated)
            {
                sceneName = levelInfo.levelName;

                // We will also want to set the "Traveling to: " text to the visual name
                travelingTo.text = "Moving to... " + levelInfo.visualName;

                levelImage.color = Color.white;
                levelImage.sprite = levelInfo.levelLoadingImage;

                StartCoroutine("LoadLevelAsync");
            }
        }
	}

    private IEnumerator LoadLevelAsync()
    {
        loadInitiated = true;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            progressBar.value = op.progress;

            yield return null;
        }
    }

    private void ShuffleTips()
    {
        System.Random rnd = new System.Random();

        int remaining = tipsToDisplay.Length;

        while (remaining > 1)
        {
            remaining--;

            int toShuffle = rnd.Next(remaining + 1);
            string value = tipsToDisplay[toShuffle];
            tipsToDisplay[toShuffle] = tipsToDisplay[remaining];
            tipsToDisplay[remaining] = value;
        }
    }

    private void UpdateTipDisplay()
    {
        currentlyDisplaying++;

        if (currentlyDisplaying >= tipsToDisplay.Length)
        {
            currentlyDisplaying = 0;
        }

        tipDisplay.text = tipsToDisplay[currentlyDisplaying];
    }
}
