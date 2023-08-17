using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public string levelName;
    public string visualName;
    public Sprite levelLoadingImage;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }
}
