
using UnityEngine;
using System;

[Serializable]
/// <summary>
/// This class is used to store all of the game, and will be loaded from an XML file
/// </summary>
public class GameSettings
{
    #region Gameplay Settings   
    /// <summary>
    /// Whether the player has done the tutorial, on first load this will be set so
    /// the tutorial will activate
    /// </summary>
    public bool tutorialActive = true;

    /// <summary>
    /// The Sensitivity of the Mouse movement in the X direction
    /// </summary>
    public float mouseXSensitivity = 35f;

    /// <summary>
    /// The Sensitivity of the Mouse movement in the Y direction
    /// </summary>
    public float mouseYSensitivity = 35f;

    public bool alternativeCameraControls = false;
    #endregion

    #region Display Settings
    /// <summary>
    /// This is the resolution the screen will display at. It stores the index of the
    /// supported resolution of the monitor we want to use. By default, it will be set to
    /// the lowest resolution that the monitor will support.
    /// </summary>
    public int screenResolution = 0;

    public bool fullscreen = true;

    public bool vSync = true;

    /// <summary>
    /// This is the amount of AA to apply to the game, and is stored as an integer index.
    /// 0 = No AA, 1 = 2x AA, 2 = 4x AA, 3 = 8x AA.
    /// </summary>
    public int antiAliasing = 0;

    /// <summary>
    /// The distance at which objects, such as props, are rendered. Stored as an integer index.
    /// 0 = 100 units, 1 = 150 units, 2 = 250 units, 3 = 300 units.
    /// </summary>
    public int renderDistance = 2;

    /// <summary>
    /// The quality of shadows to be rendered in-game. Stored as an integer index.
    /// 0 = No Shadows, 1 = Poor Shadows, 2 = Good Shadows, 3 = Awesome Shadows
    /// </summary>
    public int shadowQuality = 1;

    #endregion

    #region Sound Settings
    /// <summary>
    /// Master volume of the game. (Range 0 - 1)
    /// </summary>
    public float masterVolume = 0.8f;

    /// <summary>
    /// Volume of any in-game music. (Range 0 - 1)
    /// </summary>
    public float musicVolume = 0.8f;

    /// <summary>
    /// Volume of in-game Sound effects, such as explosions etc. (Range 0 - 1)
    /// </summary>
    public float sfxVolume = 0.8f;

    public float interfaceVolume = 0.5f;

    #endregion

    public GameSettings()
    {
        masterVolume = 0.8f;
        musicVolume = 0.8f;
        sfxVolume = 0.8f;

        interfaceVolume = 0.5f;
    }
}
