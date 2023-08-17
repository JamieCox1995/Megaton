using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuLoading : MonoBehaviour
{
    public OscilloscopeText levelName;
    public OscilloscopeText hintsNTips;

    [TextArea(5, 8)]
    public string[] hints;
    public float tipDisplayTime = 10f;
    private int currentHint = 0;

    public OscilloscopeText loadPercentage;

    private LevelInfo levelInfo;
    private string sceneName = "_MainMenu"; // Here we are setting the default value to Main Menu
    private bool loadInitiated = false;

    // Use this for initialization
    void Start()
    {
        ShuffleTips();

        InvokeRepeating("UpdateTipDisplay", 0f, tipDisplayTime);
    }

    // Update is called once per frame
    void Update()
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
                levelName.Text = levelInfo.visualName;

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
            float percentage = op.progress * 100f;

            loadPercentage.Text = string.Format("{0}%", percentage.ToString("F2"));

            yield return null;
        }
    }

    private void ShuffleTips()
    {
        System.Random rnd = new System.Random();

        int remaining = hints.Length;

        while (remaining > 1)
        {
            remaining--;

            int toShuffle = rnd.Next(remaining + 1);
            string value = hints[toShuffle];
            hints[toShuffle] = hints[remaining];
            hints[remaining] = value;
        }
    }

    private void UpdateTipDisplay()
    {
        currentHint++;

        if (currentHint >= hints.Length)
        {
            currentHint = 0;
        }

        hintsNTips.Text = hints[currentHint];
    }
}
