using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public OscilloscopeText levelName;
    public OscilloscopeText hintsNTips;

    [TextArea(5,8)]
    public string[] hints;
    public float tipDisplayTime = 10f;
    private int currentHint = 0;

    public OscilloscopeText loadPercentage;

    private string sceneToLoad;

    public void StartLoadingProcess(string levelName, string sceneName)
    {
        this.levelName.Text = levelName;
        sceneToLoad = sceneName;
    }

    private void OnEnable()
    {
        hintsNTips.Text = hints[currentHint];

        InvokeRepeating("UpdateHint", tipDisplayTime, tipDisplayTime);

        StartCoroutine(LoadLevelAsync(sceneToLoad));
    }

    private IEnumerator LoadLevelAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (op.isDone == false)
        {
            float percentage = op.progress * 100f;

            loadPercentage.Text = string.Format("{0}%", percentage.ToString("F2"));

            yield return new WaitForEndOfFrame();
        }
    }

    private void UpdateHint()
    {
        currentHint++;

        if (currentHint >= hints.Length)
        {
            currentHint = 0;
        }

        hintsNTips.Text = hints[currentHint];
    }

    private void ShuffleHints()
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
}
