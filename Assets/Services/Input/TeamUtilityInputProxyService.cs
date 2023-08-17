using System;
using System.Collections.Generic;
using TeamUtility.IO;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public class TeamUtilityInputProxyService : MonoBehaviour, IInputProxyService, IInputManager
{
    private InputManager _inputManager;
    private InputAdapter _inputAdapter;

    private bool _inputManagerNeedsConfigure;
    private bool _inputAdapterNeedsConfigure;

    private void Awake()
    {
        if (InputManager.Instance == null)
        {
            _inputManager = gameObject.AddComponent<InputManager>();
            _inputManagerNeedsConfigure = true;
        }
        if (InputAdapter.Instance == null)
        {
            _inputAdapter = gameObject.AddComponent<InputAdapter>();
            _inputAdapterNeedsConfigure = true;
        }
    }


    #region IInputProxyService implementation
    public bool simulateMouseWithTouches { get { return Input.simulateMouseWithTouches; } set { Input.simulateMouseWithTouches = value; } }
    public bool anyKey { get { return InputManager.anyKey; } }
    public bool anyKeyDown { get { return InputManager.anyKeyDown; } }
    public string inputString { get { return InputManager.inputString; } }
    public Vector3 acceleration { get { return InputManager.acceleration; } }
    public AccelerationEvent[] accelerationEvents { get { return InputManager.accelerationEvents; } }
    public int accelerationEventCount { get { return InputManager.accelerationEventCount; } }
    public Touch[] touches { get { return InputManager.touches; } }
    public int touchCount { get { return InputManager.touchCount; } }
    public bool mousePresent { get { return InputManager.mousePresent; } }
    [Obsolete("eatKeyPressOnTextFieldFocus property is deprecated, and only provided to support legacy behavior.")]
    public bool eatKeyPressOnTextFieldFocus { get { return Input.eatKeyPressOnTextFieldFocus; } set { Input.eatKeyPressOnTextFieldFocus = value; } }
    public bool stylusTouchSupported { get { return Input.stylusTouchSupported; } }
    public bool touchSupported { get { return InputManager.touchSupported; } }
    public bool multiTouchEnabled { get { return InputManager.multiTouchEnabled; } set { InputManager.multiTouchEnabled = value; } }
    public LocationService location { get { return InputManager.location; } }
    public Compass compass { get { return InputManager.compass; } }
    public DeviceOrientation deviceOrientation { get { return InputManager.deviceOrientation; } }
    public IMECompositionMode imeCompositionMode { get { return InputManager.imeCompositionMode; } set { InputManager.imeCompositionMode = value; } }
    public string compositionString { get { return InputManager.compositionString; } }
    public bool imeIsSelected { get { return InputManager.imeIsSelected; } }
    public bool touchPressureSupported { get { return Input.touchPressureSupported; } }
    public Vector2 mouseScrollDelta { get { return Input.mouseScrollDelta; } }
    public Vector3 mousePosition { get { return InputManager.mousePosition; } }
    public Gyroscope gyro { get { return InputManager.gyro; } }
    public Vector2 compositionCursorPos { get { return InputManager.compositionCursorPos; } set { InputManager.compositionCursorPos = value; } }
    public bool backButtonLeavesApp { get { return Input.backButtonLeavesApp; } set { Input.backButtonLeavesApp = value; } }
    [Obsolete("isGyroAvailable property is deprecated. Please use SystemInfo.supportsGyroscope instead.")]
    public bool isGyroAvailable { get { return Input.isGyroAvailable; } }
    public bool compensateSensors { get { return InputManager.compensateSensors; } set { InputManager.compensateSensors = value; } }
    #endregion

    #region IInputManager implementation
    public InputManager Instance { get { return InputManager.Instance; } }
    [Obsolete("Use InputManager.PlayerOneConfiguration instead", true)]
    public InputConfiguration CurrentConfiguration { get { return InputManager.CurrentConfiguration; } }
    public InputConfiguration PlayerOneConfiguration { get { return InputManager.PlayerOneConfiguration; } }
    public InputConfiguration PlayerTwoConfiguration { get { return InputManager.PlayerTwoConfiguration; } }
    public InputConfiguration PlayerThreeConfiguration { get { return InputManager.PlayerThreeConfiguration; } }
    public InputConfiguration PlayerFourConfiguration { get { return InputManager.PlayerFourConfiguration; } }
    public bool IsScanning { get { return InputManager.IsScanning; } }
    public bool IgnoreTimescale { get { return InputManager.IgnoreTimescale; } }
    public bool AnyInput() { return InputManager.AnyInput(); }
    public bool AnyInput(PlayerID playerID) { return InputManager.AnyInput(playerID); }
    public bool AnyInput(string inputConfigName) { return InputManager.AnyInput(inputConfigName); }
    public void CancelScan() { InputManager.CancelScan(); }
    public AxisConfiguration CreateAnalogAxis(string inputConfigName, string axisName, int joystick, int axis, float sensitivity, float deadZone) { return InputManager.CreateAnalogAxis(inputConfigName, axisName, joystick, axis, sensitivity, deadZone); }
    public AxisConfiguration CreateAnalogButton(string inputConfigName, string buttonName, int joystick, int axis) { return InputManager.CreateAnalogButton(inputConfigName, buttonName, joystick, axis); }
    public AxisConfiguration CreateButton(string inputConfigName, string buttonName, KeyCode primaryKey) { return InputManager.CreateButton(inputConfigName, buttonName, primaryKey); }
    public AxisConfiguration CreateButton(string inputConfigName, string buttonName, KeyCode primaryKey, KeyCode secondaryKey) { return InputManager.CreateButton(inputConfigName, buttonName, primaryKey, secondaryKey); }
    public AxisConfiguration CreateDigitalAxis(string inputConfigName, string axisName, KeyCode positive, KeyCode negative, float gravity, float sensitivity) { return InputManager.CreateDigitalAxis(inputConfigName, axisName, positive, negative, gravity, sensitivity); }
    public AxisConfiguration CreateDigitalAxis(string inputConfigName, string axisName, KeyCode positive, KeyCode negative, KeyCode altPositive, KeyCode altNegative, float gravity, float sensitivity) { return InputManager.CreateDigitalAxis(inputConfigName, axisName, positive, negative, altPositive, altNegative, gravity, sensitivity); }
    public AxisConfiguration CreateEmptyAxis(string inputConfigName, string axisName) { return InputManager.CreateEmptyAxis(inputConfigName, axisName); }
    public InputConfiguration CreateInputConfiguration(string name) { return InputManager.CreateInputConfiguration(name); }
    public AxisConfiguration CreateMouseAxis(string inputConfigName, string axisName, int axis, float sensitivity) { return InputManager.CreateMouseAxis(inputConfigName, axisName, axis, sensitivity); }
    public AxisConfiguration CreateRemoteAxis(string inputConfigName, string axisName) { return InputManager.CreateRemoteAxis(inputConfigName, axisName); }
    public AxisConfiguration CreateRemoteButton(string inputConfigName, string buttonName) { return InputManager.CreateRemoteButton(inputConfigName, buttonName); }
    public bool DeleteAxisConfiguration(string inputConfigName, string axisName) { return InputManager.DeleteAxisConfiguration(inputConfigName, axisName); }
    public bool DeleteInputConfiguration(string name) { return InputManager.DeleteInputConfiguration(name); }
    public void DisableAllAxes() { InputManager.DisableAllAxes(); }
    public void DisableAxis(string buttonName) { InputManager.DisableAxis(buttonName); }
    public void EnableAllAxes() { InputManager.EnableAllAxes(); }
    public void EnableAxis(string buttonName) { InputManager.EnableAxis(buttonName); }
    public AccelerationEvent GetAccelerationEvent(int index) { return InputManager.GetAccelerationEvent(index); }
    public float GetAxis(string axisName) { return InputManager.GetAxis(axisName); }
    public AxisConfiguration GetAxisConfiguration(string inputConfigName, string axisName) { return InputManager.GetAxisConfiguration(inputConfigName, axisName); }
    public AxisConfiguration GetAxisConfiguration(PlayerID playerID, string axisName) { return InputManager.GetAxisConfiguration(playerID, axisName); }
    public float GetAxisRaw(string axisName) { return InputManager.GetAxisRaw(axisName); }
    public bool GetButton(string buttonName) { return InputManager.GetButton(buttonName); }
    public bool GetButtonDown(string buttonName) { return InputManager.GetButtonDown(buttonName); }
    public bool GetButtonUp(string buttonName) { return InputManager.GetButtonUp(buttonName); }
    public InputConfiguration GetInputConfiguration(string name) { return InputManager.GetInputConfiguration(name); }
    public InputConfiguration GetInputConfiguration(PlayerID playerID) { return InputManager.GetInputConfiguration(playerID); }
    public string[] GetJoystickNames() { return InputManager.GetJoystickNames(); }
    public bool GetKey(string name)
    {
        KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), name, true);
        return InputManager.GetKey(key);
    }
    public bool GetKey(KeyCode key) { return InputManager.GetKey(key); }
    public bool GetKeyDown(KeyCode key) { return InputManager.GetKeyDown(key); }
    public bool GetKeyDown(string name)
    {
        KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), name, true);
        return InputManager.GetKeyDown(key);
    }
    public bool GetKeyUp(KeyCode key) { return InputManager.GetKeyUp(key); }
    public bool GetKeyUp(string name)
    {
        KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), name, true);
        return InputManager.GetKeyUp(key);
    }
    public bool GetMouseButton(int button) { return InputManager.GetMouseButton(button); }
    public bool GetMouseButtonDown(int button) { return InputManager.GetMouseButtonDown(button); }
    public bool GetMouseButtonUp(int button) { return InputManager.GetMouseButtonUp(button); }
    public Touch GetTouch(int index) { return InputManager.GetTouch(index); }
#if UNITY_STANDALONE_LINUX
    public bool IsJoystickPreconfigured(string joystickName) { return Input.IsJoystickPreconfigured(joystickName); }
#endif
    public void Load() { InputManager.Load(); }
    public void Load(string filename) { InputManager.Load(filename); }
    public void Load(IInputLoader inputLoader) { InputManager.Load(inputLoader); }
    public void Reinitialize() { InputManager.Reinitialize(); }
    public void ResetInputAxes() { InputManager.ResetInputAxes(); }
    public void ResetInputConfiguration(PlayerID playerID) { InputManager.ResetInputConfiguration(playerID); }
    public void Save() { InputManager.Save(); }
    public void Save(string filename) { InputManager.Save(filename); }
    public void Save(IInputSaver inputSaver) { InputManager.Save(inputSaver); }
    public void SetConfigurationDirty(string inputConfigName) { InputManager.SetConfigurationDirty(inputConfigName); }
    [Obsolete("Use the method overload that takes in the player ID", true)]
    public void SetInputConfiguration(string name) { InputManager.SetInputConfiguration(name); }
    public void SetInputConfiguration(string name, PlayerID playerID) { InputManager.SetInputConfiguration(name, playerID); }
    [Obsolete("Use the method overload that takes in the input configuration name", true)]
    public void SetRemoteAxisValue(string axisName, float value) { InputManager.SetRemoteAxisValue(axisName, value); }
    public void SetRemoteAxisValue(string inputConfigName, string axisName, float value) { InputManager.SetRemoteAxisValue(inputConfigName, axisName, value); }
    [Obsolete("Use the method overload that takes in the input configuration name", true)]
    public void SetRemoteButtonValue(string buttonName, bool down, bool justChanged) { InputManager.SetRemoteButtonValue(buttonName, down, justChanged); }
    public void SetRemoteButtonValue(string inputConfigName, string buttonName, bool down, bool justChanged) { InputManager.SetRemoteButtonValue(inputConfigName, buttonName, down, justChanged); }
    public void StartJoystickAxisScan(AxisScanHandler scanHandler, int? joystick, float timeout, string cancelScanButton, params object[] userData) { InputManager.StartJoystickAxisScan(scanHandler, joystick, timeout, cancelScanButton, userData); }
    public void StartJoystickButtonScan(KeyScanHandler scanHandler, int? joystick, float timeout, string cancelScanButton, params object[] userData) { InputManager.StartJoystickButtonScan(scanHandler, joystick, timeout, cancelScanButton, userData); }
    public void StartKeyboardButtonScan(KeyScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData) { InputManager.StartKeyboardButtonScan(scanHandler, timeout, cancelScanButton, userData); }
    [Obsolete("Use StartKeyboardButtonScan or StartJoystickButtonScan instead.")]
    public void StartKeyScan(KeyScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData) { InputManager.StartKeyScan(scanHandler, timeout, cancelScanButton, userData); }
    public void StartMouseAxisScan(AxisScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData) { InputManager.StartMouseAxisScan(scanHandler, timeout, cancelScanButton, userData); }
    public void StartScan(ScanSettings settings, ScanHandler scanHandler) { InputManager.StartScan(settings, scanHandler); }

    public void Configure(InputManagerConfiguration configuration)
    {
        if (_inputManagerNeedsConfigure) ConfigureInputManager(configuration);
        if (_inputAdapterNeedsConfigure) ConfigureInputAdapter(configuration);
    }

    private void ConfigureInputManager(InputManagerConfiguration configuration)
    {
        string dir = System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Documents\\Megaton");

        InputManager.Load(System.IO.Path.Combine(dir, "input_config.xml"));
    }

    private void ConfigureInputAdapter(InputManagerConfiguration configuration)
    {
        var bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

        var values = new Dictionary<string, string>
        {
            { "keyboardConfiguration", configuration.KeyboardConfiguration },
            { "windowsJoystickConfiguration", configuration.WindowsJoystickConfiguration },
            { "osxJoystickConfiguration", configuration.OsxJoystickConfiguration },
            { "linuxJoystickConfiguration", configuration.LinuxJoystickConfiguration },
            { "leftTriggerAxis", configuration.LeftTriggerAxis },
            { "rightTriggerAxis", configuration.RightTriggerAxis },
            { "dpadHorizontalAxis", configuration.DpadHorizontalAxis },
            { "dpadVerticalAxis", configuration.DpadVerticalAxis },
        };

        foreach (var kvp in values)
        {
            if (string.IsNullOrEmpty(kvp.Value)) continue;

            var fieldInfo = typeof(InputAdapter).GetField(kvp.Key, bindingFlags);
            fieldInfo.SetValue(_inputAdapter, kvp.Value);
        }

        typeof(InputAdapter).GetMethod("SetInputManagerConfigurations", bindingFlags).Invoke(_inputAdapter, null);
        typeof(InputAdapter).GetMethod("SetInputDevice", bindingFlags).Invoke(_inputAdapter, new object[] { InputDevice.KeyboardAndMouse });
    }
#endregion
}
