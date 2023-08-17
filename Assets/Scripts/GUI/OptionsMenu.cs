using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;

public class OptionsMenu : MonoBehaviour
{
    #region References
    [Header("Video UI Elements")]
    public Toggle fullscreen;
    private bool isFullscreen;

    public Dropdown resolutionDropdown;
    private Resolution[] monitorResolutions;

    private Resolution previousResolution;
    private bool previousFullscreen;

    public Toggle vsyncToggle;
    private int vsync;

    [Header("AA Settings")]
    public Dropdown aaDropdown;
    private int previousAASetting;

    [Header("Shadow Settings")]
    public Dropdown shadowDropdown;
    public ShadowQualitySettings[] shadowQualityLevels;
    private int currentShadowSetting;
    private int previousShadowSetting;

    [Header("Render Distance Settings")]
    public Dropdown renderDistanceDropdown;
    public float[] renderDistances;

    [Header("Audio UI Elements")]
    public AudioMixer mixer;
    public Slider masterSlider;
    public Text masterDispaly;
    public Slider musicSlider;
    public Text musicDisplay;
    public Slider sfxSlider;
    public Text sfxDisplay;
    public Slider interfaceSlider;
    public Text interfaceDisplay;

    [Header("Mouse Settings")]
    public Slider mouseXSlider;
    public Text mouseXDisplay;

    public Slider mouseYSlider;
    public Text mouseYDisplay;

    [Header("Alternate Camera Mode Settings")]
    public Toggle useAltCamera;

    [Header("Confirmation Dialogue")]
    public GameObject confirmationDialogue;
    private bool videoSettingsChanged = false;  // We are going to use this value to decide whether we should show the confirmation dialogue


    #endregion

    [Header("Options Accepted Events")]
    public UnityEvent onBaseAccept;
    public UnityEvent onModifiedAccept;

    private string dataPath = System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Documents\\Megaton");

    // Use this for initialization
    void Start ()
    {
        GetValues();

        SetSavedShadowSettings();

        shadowDropdown.value = SettingsManager._instance.gameSettings.shadowQuality;
        shadowDropdown.RefreshShownValue();

        dataPath = System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Documents\\Megaton");
    }

    public void GetValues()
    {
        GetFullscreen();
        GetResolutions();
        GetVsync();

        GetAA();

        SetSavedShadowSettings();
        GetCurrentShadowSettings();

        GetRenderDistance();

        GetSoundSettings();
        GetMouseX();
        GetMouseY();

        GetAltCameraMode();
    }

    public void ChangingVideoSettings()
    {
        videoSettingsChanged = true;
    }

    public void ResetVideoBool()
    {
        videoSettingsChanged = false;
    }

    public void SetSavedShadowSettings()
    {
        ChangeShadowQuality(shadowQualityLevels[SettingsManager._instance.gameSettings.shadowQuality]);
    }

    private void GetFullscreen()
    {
        isFullscreen = SettingsManager._instance.gameSettings.fullscreen;

        fullscreen.isOn = isFullscreen;
    }

    private void GetResolutions()
    {
        resolutionDropdown.ClearOptions();

        monitorResolutions = Screen.resolutions;

        List<string> resolutions = new List<string>();

        foreach (Resolution res in monitorResolutions)
        {
            string resDisplay = res.width + " x " + res.height + " @ " + res.refreshRate + "Hz";

            resolutions.Add(resDisplay);
        }

        resolutionDropdown.AddOptions(resolutions);

        resolutionDropdown.value = SettingsManager._instance.gameSettings.screenResolution;
        resolutionDropdown.RefreshShownValue();
    }

    private void GetVsync()
    {
        vsyncToggle.isOn = SettingsManager._instance.gameSettings.vSync;
    }

    private void GetSoundSettings()
    {
        float master = SettingsManager._instance.gameSettings.masterVolume;
        masterDispaly.text = master.ToString("F2");
        masterSlider.value = master;
        
        mixer.SetFloat("Master Volume", (masterSlider.value * 100f) - 80f);


        float music = SettingsManager._instance.gameSettings.musicVolume;
        musicDisplay.text = music.ToString("F2");
        musicSlider.value = music;

        mixer.SetFloat("Music Volume", (musicSlider.value * 100f) - 80f);


        float sfx = SettingsManager._instance.gameSettings.sfxVolume;
        sfxDisplay.text = sfx.ToString("F2");
        sfxSlider.value = sfx;

        mixer.SetFloat("SFX Volume", (sfxSlider.value * 100f) - 80f);

        float interfaceV = SettingsManager._instance.gameSettings.interfaceVolume;
        interfaceDisplay.text = interfaceV.ToString("F2");
        interfaceSlider.value = interfaceV;

        mixer.SetFloat("Interface Volume", (interfaceV * 100f) - 80f);

        //print("Master: " + master + ", Music: " + music + ", SFX: " + sfx);

    }

    public void ChangeMasterDisplay()
    {
        SettingsManager._instance.gameSettings.masterVolume = masterSlider.value;

        masterDispaly.text = masterSlider.value.ToString("F2");

        mixer.SetFloat("Master Volume", (masterSlider.value * 100f) - 80f);
    }

    public void ChangeMusicDisplay()
    {
        SettingsManager._instance.gameSettings.musicVolume = musicSlider.value;

        musicDisplay.text = musicSlider.value.ToString("F2");

        mixer.SetFloat("Music Volume", (musicSlider.value * 100f) - 80f);
    }

    public void ChangeSFXDisplay()
    {
        SettingsManager._instance.gameSettings.sfxVolume = sfxSlider.value;

        sfxDisplay.text = sfxSlider.value.ToString("F2");

        mixer.SetFloat("SFX Volume", (sfxSlider.value * 100f) - 80f);
    }

    public void ChangeInterfaceDisplay()
    {
        // Update the Settings Manager
        SettingsManager._instance.gameSettings.interfaceVolume = interfaceSlider.value;

        interfaceDisplay.text = interfaceSlider.value.ToString("F2");

        mixer.SetFloat("Interface Volume", (interfaceSlider.value * 100f) - 80f);
    }

    private void GetMouseX()
    {
        float x = (SettingsManager._instance.gameSettings.mouseXSensitivity - 0.25f) * 50f;

        mouseXSlider.value = x;
        mouseXDisplay.text = x.ToString("F0");
    }

    private void GetMouseY()
    {
        float y = (SettingsManager._instance.gameSettings.mouseYSensitivity - 0.25f) * 50f;

        mouseYSlider.value = y;
        mouseYDisplay.text = y.ToString("F0");
    }

    public void ChangeMouseX()
    {
        float value = (mouseXSlider.value / 50f) + 0.25f;

        SettingsManager._instance.gameSettings.mouseXSensitivity = value;

        mouseXDisplay.text = mouseXSlider.value.ToString("F0");
    }

    public void ChangeMouseY()
    {
        float value = (mouseYSlider.value / 50f) + 0.25f;

        SettingsManager._instance.gameSettings.mouseYSensitivity = value;

        mouseYDisplay.text = mouseYSlider.value.ToString("F0");
    }

    private void GetAltCameraMode()
    {
        useAltCamera.isOn = SettingsManager._instance.gameSettings.alternativeCameraControls;
    }

    public void ChangeCameraMode()
    {
        SettingsManager._instance.gameSettings.alternativeCameraControls = useAltCamera.isOn;
    }

    public void ApplySettings()
    {
        if (videoSettingsChanged)
        {
            SetScreenResolution();

            SetAASetting();

            SetShadowSettings();

            StartCoroutine("DiscardCountdown");
        }
        else
        {
            SettingsManager.Save();

            if (GameEventManager.instance != null) GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnSettingsChanged));

            onBaseAccept.Invoke();
        }
    }

    public void DiscardChanges()
    {
        StopCoroutine("DiscardCountdown");

        Screen.SetResolution(previousResolution.width, previousResolution.height, previousFullscreen);

        ChangeShadowQuality(shadowQualityLevels[previousShadowSetting]);

        QualitySettings.antiAliasing = previousAASetting;

        videoSettingsChanged = false;

        confirmationDialogue.SetActive(false);
    }

    public void KeepChanges()
    {
        StopCoroutine("DiscardCountdown");

        SettingsManager._instance.gameSettings.antiAliasing = aaDropdown.value;
        SettingsManager._instance.gameSettings.fullscreen = fullscreen.isOn;

        SettingsManager._instance.gameSettings.screenResolution = resolutionDropdown.value;
        SettingsManager._instance.gameSettings.shadowQuality = shadowDropdown.value;

        SettingsManager._instance.gameSettings.renderDistance = renderDistanceDropdown.value;

        SettingsManager._instance.gameSettings.alternativeCameraControls = useAltCamera.isOn;

        SettingsManager.Save();

        if (GameEventManager.instance != null) GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnSettingsChanged));

        videoSettingsChanged = false;

        onModifiedAccept.Invoke();

        //confirmationDialogue.SetActive(false);
    }

    private IEnumerator DiscardCountdown()
    {
        confirmationDialogue.SetActive(true);

        float timer = 0f;

        while (timer < 15f)
        {
            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        DiscardChanges();
    }


    private void SetScreenResolution()
    {
        previousResolution = Screen.currentResolution;
        previousFullscreen = !fullscreen.isOn;

        int selectedRes = resolutionDropdown.value;
        Resolution res = monitorResolutions[selectedRes];

        Screen.SetResolution(res.width, res.height, fullscreen.isOn);

        videoSettingsChanged = true;
    }

    private void GetAA()
    {
        int currentAA = QualitySettings.antiAliasing;

        currentAA = SettingsManager._instance.gameSettings.antiAliasing;

        switch(currentAA)
        {
            case 0:
                aaDropdown.value = 0;
                break;
            case 2:
                aaDropdown.value = 1;
                break;
            case 4:
                aaDropdown.value = 2;
                break;
            case 8:
                aaDropdown.value = 3;
                break;
        }
    }

    private void SetAASetting()
    {
        previousAASetting = QualitySettings.antiAliasing;

        int selected = aaDropdown.value;

        switch(selected)
        {
            case 0:
                QualitySettings.antiAliasing = 0;
                break;
            case 1:
                QualitySettings.antiAliasing = 2;
                break;
            case 2:
                QualitySettings.antiAliasing = 4;
                break;
            case 3:
                QualitySettings.antiAliasing = 8;
                break;
        }

        videoSettingsChanged = true;
    }

    private void GetCurrentShadowSettings()
    {
        ShadowQualitySettings currentSettings = new ShadowQualitySettings(QualitySettings.shadows, QualitySettings.shadowResolution, QualitySettings.shadowProjection,
            QualitySettings.shadowDistance, QualitySettings.shadowNearPlaneOffset, QualitySettings.shadowCascades);

        for(int i = 0; i < shadowQualityLevels.Length; i++)
        {
            if (currentSettings.Equals(shadowQualityLevels[i]))
            {
                currentShadowSetting = i;

                shadowDropdown.value = i;

                return;
            }
        }

        shadowDropdown.RefreshShownValue();
    }

    private void SetShadowSettings()
    {
        previousShadowSetting = currentShadowSetting;

        int settingIndex = shadowDropdown.value;
        currentShadowSetting = settingIndex;

        ShadowQualitySettings setting = shadowQualityLevels[settingIndex];

        ChangeShadowQuality(setting);

        videoSettingsChanged = true;
    }

    private void ChangeShadowQuality(ShadowQualitySettings setting)
    {
        QualitySettings.shadows = setting.shadowQuality;
        QualitySettings.shadowResolution = setting.shadowResolution;
        QualitySettings.shadowProjection = setting.shadowProjection;
        QualitySettings.shadowDistance = setting.shadowDist;
        QualitySettings.shadowNearPlaneOffset = setting.nearClipShadow;
        QualitySettings.shadowCascades = setting.shadowCascades;
    }

    private void GetRenderDistance()
    {
        renderDistanceDropdown.value = SettingsManager._instance.gameSettings.renderDistance;
        renderDistanceDropdown.RefreshShownValue();
    }
}

[System.Serializable]
public struct ShadowQualitySettings
{
    public ShadowQuality shadowQuality;
    public ShadowResolution shadowResolution;
    public ShadowProjection shadowProjection;
    public float shadowDist;
    public float nearClipShadow;
    public int shadowCascades;

    public ShadowQualitySettings(ShadowQuality qual, ShadowResolution res, ShadowProjection proj, float distance, float offset, int cascades)
    {
        shadowQuality = qual;
        shadowResolution = res;
        shadowProjection = proj;
        shadowDist = distance;
        nearClipShadow = offset;
        shadowCascades = cascades;
    }
}
