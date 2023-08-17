using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class OsciliscopeSettingsMenu : MonoBehaviour
{
    #region UI Components
    [Header("Window Settings Components:")]
    public OscilloscopeDropdown resolutionDropdown;
    public OscilloscopeToggle fullscreenToggle;
    public OscilloscopeToggle vsyncToggle;

    [Header("Interface Settings Components:")]
    public OscilloscopeSlider interfaceColourSlider;

    [Header("Quality Settings Components:")]
    public OscilloscopeDropdown antialiasingDropdown;
    public OscilloscopeDropdown shadowQualityDropdown;
    public OscilloscopeDropdown renderDistanceDropdown;

    [Header("Audio Settings Components:")]
    public OscilloscopeSlider masterVolumeSlider;
    public OscilloscopeSlider musicVolumeSlider;
    public OscilloscopeSlider sfxVolumeSlider;
    public OscilloscopeSlider interfaceVolumeSlider;

    [Header("Gameplay Settings Components:")]
    public OscilloscopeSlider horizontalMouseSlider;
    public OscilloscopeSlider verticalMouseSlider;
    public OscilloscopeToggle altCameraToggle;

    [Header("Menu Components:")]
    public OscilloscopeDropdown menuDropdown;
    public OscilloscopeButton saveButton;
    #endregion

    public ShadowQualitySettings[] shadowQualityLevels;
    public PhosphorScreenRenderer phosphorScreen;

    public Animator menuAnimator;
    public AudioMixer audioMixer;

    [Header("Keep/Discard Settings:")]
    public GameObject dialoguePanel;
    public OscilloscopeText discardText;

    public UnityEvent onModifiedAccept;
    public UnityEvent onBaseAccept;

    private bool videoSettingsChanged = false;

    // Here we are storing some of the previous settibngs so we can revert if needs be
    private Resolution previousResolution;
    private bool wasFullscreen;
    private bool wasVSync;

    private ShadowQualitySettings previousShadow;
    private int previousRenderDistance;
    private int previousAA;

    // Use this for initialization
    void Start ()
    {
        RetrieveSavedValues();
	}

    #region Retrieving Saved Values
    public void RetrieveSavedValues()
    {
        RetrieveWindowSettings();
        RetrieveQualitySettings();
        RetrieveInterfaceSettings();

        RetrieveAudioSettings();

        RetrieveGameplaySettings();
    }

    private void RetrieveWindowSettings()
    {
        // Getting the monitors supported resolutions and currently set resolution
        Resolution current = Screen.currentResolution;
        List<Resolution> supported = Screen.resolutions.ToList();
        int index = supported.IndexOf(current);

        resolutionDropdown.ClearOptions();

        List<string> list = new List<string>();
        foreach (Resolution res in supported)
        {
            string display = string.Format("{0}x{1} @ {2}Hz", res.width, res.height, res.refreshRate);

            list.Add(display);
        }

        resolutionDropdown.AddOptions(list);
        resolutionDropdown.value = index;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.CaptionText.Text = list[index];

        // Getting if the game should be in fullscreen
        bool isFullscreen = SettingsManager._instance.gameSettings.fullscreen;

        fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;

        // Finally we are getting the value for if the player wants VSyn
        bool vsyncOn = SettingsManager._instance.gameSettings.vSync;

        vsyncToggle.isOn = vsyncOn;
        QualitySettings.vSyncCount = (vsyncOn == true) ? 1 : 0;
    }

    private void RetrieveQualitySettings()
    {
        // Getting the current AA settings
        int currentAA = SettingsManager._instance.gameSettings.antiAliasing;

        switch(currentAA)
        {
            case 0:
                antialiasingDropdown.value = 0;
                break;
            case 2:
                antialiasingDropdown.value = 1;
                break;
            case 4:
                antialiasingDropdown.value = 2;
                break;
            case 8:
                antialiasingDropdown.value = 3;
                break;
        }

        QualitySettings.antiAliasing = currentAA;

        antialiasingDropdown.RefreshShownValue();
        antialiasingDropdown.CaptionText.Text = antialiasingDropdown.options[antialiasingDropdown.value].text;

        // Getting the current shadow settings
        int currentShadow = SettingsManager._instance.gameSettings.shadowQuality;

        shadowQualityDropdown.value = currentShadow;
        shadowQualityDropdown.CaptionText.Text = shadowQualityDropdown.options[currentShadow].text;

        SetShadowSettings(shadowQualityLevels[currentShadow]);

        // Getting the Render Distance.
        int currentRenderDistance = SettingsManager._instance.gameSettings.renderDistance;

        renderDistanceDropdown.value = currentRenderDistance;
        renderDistanceDropdown.RefreshShownValue();

        renderDistanceDropdown.CaptionText.Text = renderDistanceDropdown.options[currentRenderDistance].text;
    }

    private void RetrieveInterfaceSettings()
    {
        float interfaceHue = 0.1f;

        if (PlayerPrefs.HasKey("InterfaceHue"))
        {
            // Setting the colour of the UI
            interfaceHue = PlayerPrefs.GetFloat("InterfaceHue");
        }
        else
        {
            float defaultHue = 0.1f;

            PlayerPrefs.SetFloat("InterfaceHue", defaultHue);
        }

        interfaceColourSlider.value = interfaceHue;

        Color retrievedColour = Color.HSVToRGB(interfaceHue, 0.9f, 1f);
        retrievedColour.a = 0.6f;
        //Debug.LogFormat("RetrieveColour was: {0}", retrievedColour);

        phosphorScreen.PhosphorColor = retrievedColour;
    }

    private void RetrieveAudioSettings()
    {
        // Getting the Master Audio Settings
        float currentVolume = SettingsManager._instance.gameSettings.masterVolume;

        // Now we set the Slider and audio mixer settings
        masterVolumeSlider.value = currentVolume;
        audioMixer.SetFloat("Master Volume", (currentVolume * 100f) - 80f);

        // Music Volume
        currentVolume = SettingsManager._instance.gameSettings.musicVolume;

        musicVolumeSlider.value = currentVolume;
        audioMixer.SetFloat("Music Volume", (currentVolume * 100f) - 80f);

        // SFX Volume
        currentVolume = SettingsManager._instance.gameSettings.sfxVolume;

        sfxVolumeSlider.value = currentVolume;
        audioMixer.SetFloat("SFX Volume", (currentVolume * 100f) - 80f);

        // Interface Volume
        currentVolume = SettingsManager._instance.gameSettings.interfaceVolume;

        interfaceVolumeSlider.value = currentVolume;
        audioMixer.SetFloat("Interface Volume", (currentVolume * 100f) - 80f);
    }

    private void RetrieveGameplaySettings()
    {
        float mouseSens = (SettingsManager._instance.gameSettings.mouseXSensitivity - 0.25f) * 50f;
        horizontalMouseSlider.value = mouseSens;

        mouseSens = (SettingsManager._instance.gameSettings.mouseYSensitivity - 0.25f) * 50f;
        verticalMouseSlider.value = mouseSens;

        bool useAlt = SettingsManager._instance.gameSettings.alternativeCameraControls;
        altCameraToggle.isOn = useAlt;
    }
    #endregion

    private void SetShadowSettings(ShadowQualitySettings setting)
    {
        QualitySettings.shadows = setting.shadowQuality;
        QualitySettings.shadowResolution = setting.shadowResolution;
        QualitySettings.shadowProjection = setting.shadowProjection;
        QualitySettings.shadowDistance = setting.shadowDist;
        QualitySettings.shadowNearPlaneOffset = setting.nearClipShadow;
        QualitySettings.shadowCascades = setting.shadowCascades;
    }

    public void SetNewWindowSettings()
    {
        previousResolution = Screen.currentResolution;
        wasFullscreen = fullscreenToggle.isOn;
        wasVSync = vsyncToggle.isOn;

        // Setting the new Screen Resolution and Fullscreen mode and VSync
        Resolution newResolution = Screen.resolutions[resolutionDropdown.value];
        Screen.SetResolution(newResolution.width, newResolution.height, fullscreenToggle.isOn, newResolution.refreshRate);
        QualitySettings.vSyncCount = (vsyncToggle.isOn == true) ? 1 : 0;

        // Updating all of the Settings manager values
        SettingsManager._instance.gameSettings.shadowQuality = shadowQualityDropdown.value;
        SettingsManager._instance.gameSettings.fullscreen = fullscreenToggle.isOn;
        SettingsManager._instance.gameSettings.vSync = vsyncToggle.isOn;
    }

    public void OnFullscreenToggled()
    {
        wasFullscreen = !fullscreenToggle.isOn;
    }

    public void OnVSyncToggle()
    {
        wasVSync = !vsyncToggle.isOn;
    }

	public void OnInterfaceHueChanged(float value)
    {
        PlayerPrefs.SetFloat("InterfaceHue", value);

        /*phosphorScreen.PhosphorColor = Color.HSVToRGB(value, 0.9f, 1f);
        phosphorScreen.PhosphorColor.a = 0.6f;*/
    }

    public void SetQualitySettings()
    {
        // Setting the previous Shadow setting and updating the QualitySettings
        previousShadow = shadowQualityLevels[SettingsManager._instance.gameSettings.shadowQuality];
        SetShadowSettings(shadowQualityLevels[shadowQualityDropdown.value]);
        SettingsManager._instance.gameSettings.shadowQuality = shadowQualityDropdown.value;

        previousAA = SettingsManager._instance.gameSettings.antiAliasing;
        QualitySettings.antiAliasing = antialiasingDropdown.value;
        SettingsManager._instance.gameSettings.antiAliasing = antialiasingDropdown.value;

        previousRenderDistance = SettingsManager._instance.gameSettings.renderDistance;
        SettingsManager._instance.gameSettings.renderDistance = renderDistanceDropdown.value;
    }

    public void OnVideoSettingChanged()
    {
        videoSettingsChanged = true;
    }

    public void OnSettingPanelChanged(int value)
    {
        switch(value)
        {
            case 0:
                menuAnimator.SetTrigger("Window Settings");
                break;
            case 1:
                menuAnimator.SetTrigger("Quality Settings");
                break;
            case 2:
                menuAnimator.SetTrigger("Interface Settings");
                break;
            case 3:
                menuAnimator.SetTrigger("Audio Settings");
                break;
            case 4:
                menuAnimator.SetTrigger("Gameplay Settings");
                break;
        }
    }

    public void OnMasterVolumeChanged()
    {
        float value = masterVolumeSlider.value;

        audioMixer.SetFloat("Master Volume", (value * 100f) - 80f);

        SettingsManager._instance.gameSettings.masterVolume = value;
    }

    public void OnMusicVolumeChanged()
    {
        float value = musicVolumeSlider.value;

        audioMixer.SetFloat("Music Volume", (value * 100f) - 80f);

        SettingsManager._instance.gameSettings.musicVolume = value;
    }

    public void OnSFXVolumeChanged()
    {
        float value = sfxVolumeSlider.value;

        audioMixer.SetFloat("SFX Volume", (value * 100f) - 80f);

        SettingsManager._instance.gameSettings.sfxVolume = value;
    }

    public void OnInterfaceVolumeChanged()
    {
        float value = interfaceVolumeSlider.value;

        audioMixer.SetFloat("Interface Volume", (value * 100f) - 80f);

        SettingsManager._instance.gameSettings.interfaceVolume = value;
    }

    public void OnAltCameraChanged()
    {
        SettingsManager._instance.gameSettings.alternativeCameraControls = altCameraToggle.isOn ;
    }

    public void OnHorizontalSliderChanged()
    {
        float value = horizontalMouseSlider.value;

        SettingsManager._instance.gameSettings.mouseXSensitivity = (value / 50f) + 0.25f;
    }

    public void OnVerticalSliderChanged()
    {
        float value = verticalMouseSlider.value;

        SettingsManager._instance.gameSettings.mouseYSensitivity = (value / 50f) + 0.25f;
    }

    public void OnSettingsOpened()
    {
        RetrieveSavedValues();

        menuDropdown.value = 0;
        menuDropdown.RefreshShownValue();

        videoSettingsChanged = false;

        menuDropdown.CaptionText.Text = menuDropdown.options[0].text;
    }

    public void ApplySettings()
    {
        if (videoSettingsChanged)
        {
            SetNewWindowSettings();

            SetQualitySettings();

            StartCoroutine("StartDiscard");
        }
        else
        {
            SettingsManager.Save();

            if (GameEventManager.instance != null) GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnSettingsChanged));

            onBaseAccept.Invoke();
        }
    }

    private IEnumerator StartDiscard()
    {
        // We want to open the confirmation dialogue.
        dialoguePanel.SetActive(true);

        // We want to countdown a timer.
        float timer = 15f;

        while (timer >= 0f)
        {
            timer -= Time.deltaTime;

            // Update the countdown text.
            string discardString = string.Format("Keep new changes? Changes will automatically discard in {0} seconds.", timer.ToString("F0"));
            discardText.Text = discardString;

            yield return new WaitForEndOfFrame();
        }

        // Calling the Discard method.
        DiscardChanges();
    }

    public void DiscardPressed()
    {
        StopCoroutine("StartDiscard");

        DiscardChanges();
    }

    public void DiscardChanges()
    {

        // Resolution and Fullscreen
        resolutionDropdown.value = Screen.resolutions.ToList().IndexOf(previousResolution);
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = wasFullscreen;
        SettingsManager._instance.gameSettings.fullscreen = fullscreenToggle.isOn;

        Screen.SetResolution(previousResolution.width, previousResolution.height, wasFullscreen, previousResolution.refreshRate);
        SettingsManager._instance.gameSettings.screenResolution = Screen.resolutions.ToList().IndexOf(previousResolution);

        // VSync
        vsyncToggle.isOn = wasVSync;
        SettingsManager._instance.gameSettings.vSync = wasVSync;

        // Shadow Settings
        shadowQualityDropdown.value = shadowQualityLevels.ToList().IndexOf(previousShadow);
        shadowQualityDropdown.RefreshShownValue();

        SetShadowSettings(previousShadow);
        SettingsManager._instance.gameSettings.shadowQuality = shadowQualityLevels.ToList().IndexOf(previousShadow);

        // Anti-Aliasing


        videoSettingsChanged = false;

        // Close the Dialogue
        dialoguePanel.SetActive(false);
    }

    public void KeepChanges()
    {
        StopCoroutine("StartDiscard");

        SettingsManager.Save();

        if (GameEventManager.instance != null) GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnSettingsChanged));

        videoSettingsChanged = false;
        dialoguePanel.SetActive(false);
    }
}
