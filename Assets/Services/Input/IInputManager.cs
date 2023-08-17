using System;
using TeamUtility.IO;
using TotalDistraction.ServiceLocation;
using UnityEngine;

public interface IInputManager : IUnityService<InputManagerConfiguration>
{
    InputManager Instance { get; }

    [Obsolete("Use InputManager.PlayerOneConfiguration instead", true)]
    InputConfiguration CurrentConfiguration { get; }

    InputConfiguration PlayerOneConfiguration { get; }
    InputConfiguration PlayerTwoConfiguration { get; }
    InputConfiguration PlayerThreeConfiguration { get; }
    InputConfiguration PlayerFourConfiguration { get; }
    bool IsScanning { get; }
    bool IgnoreTimescale { get; }

    /// <summary>
    /// Returns true if any axis of any active input configuration is receiving input.
    /// </summary>
    bool AnyInput();

    /// <summary>
	/// Returns true if any axis of the input configuration is receiving input.
	/// </summary>
    bool AnyInput(PlayerID playerID);

    /// <summary>
    /// Returns true if any axis of the specified input configuration is receiving input.
    /// If the specified input configuration is not active and the axis is of type
    /// DigialAxis, RemoteAxis, RemoteButton or AnalogButton this method will return false.
    /// </summary>
    bool AnyInput(string inputConfigName);

    /// <summary>
    /// If an axis with the requested name exists, and it is of type 'RemoteAxis', the axis' value will be changed.
    /// </summary>
    [Obsolete("Use the method overload that takes in the input configuration name", true)]
    void SetRemoteAxisValue(string axisName, float value);

    /// <summary>
    /// If an axis with the requested name exists, and it is of type 'RemoteAxis', the axis' value will be changed.
    /// </summary>
    void SetRemoteAxisValue(string inputConfigName, string axisName, float value);

    /// <summary>
    /// If an button with the requested name exists, and it is of type 'RemoteButton', the button's state will be changed.
    /// </summary>
	[Obsolete("Use the method overload that takes in the input configuration name", true)]
    void SetRemoteButtonValue(string buttonName, bool down, bool justChanged);

    /// <summary>
    /// If an button with the requested name exists, and it is of type 'RemoteButton', the button's state will be changed.
    /// </summary>
    void SetRemoteButtonValue(string inputConfigName, string buttonName, bool down, bool justChanged);

    /// <summary>
    /// Resets the internal state of the input manager.
    /// </summary>
    void Reinitialize();

    void ResetInputConfiguration(PlayerID playerID);

    /// <summary>
    /// Changes the active input configuration.
    /// </summary>
	[Obsolete("Use the method overload that takes in the player ID", true)]
    void SetInputConfiguration(string name);

    /// <summary>
    /// Changes the active input configuration.
    /// </summary>
    void SetInputConfiguration(string name, PlayerID playerID);

    InputConfiguration GetInputConfiguration(string name);

    InputConfiguration GetInputConfiguration(PlayerID playerID);

    AxisConfiguration GetAxisConfiguration(string inputConfigName, string axisName);

    AxisConfiguration GetAxisConfiguration(PlayerID playerID, string axisName);

    InputConfiguration CreateInputConfiguration(string name);

    /// <summary>
    /// Deletes the specified input configuration. If the speficied input configuration is
    /// active for any player then the active input configuration for the respective player will be set to null.
    /// </summary>
    bool DeleteInputConfiguration(string name);

    AxisConfiguration CreateButton(string inputConfigName, string buttonName, KeyCode primaryKey);

    AxisConfiguration CreateButton(string inputConfigName, string buttonName, KeyCode primaryKey, KeyCode secondaryKey);

    AxisConfiguration CreateDigitalAxis(string inputConfigName, string axisName, KeyCode positive, KeyCode negative, float gravity, float sensitivity);

    AxisConfiguration CreateDigitalAxis(string inputConfigName, string axisName, KeyCode positive, KeyCode negative,
                                                        KeyCode altPositive, KeyCode altNegative, float gravity, float sensitivity);

    AxisConfiguration CreateMouseAxis(string inputConfigName, string axisName, int axis, float sensitivity);

    AxisConfiguration CreateAnalogAxis(string inputConfigName, string axisName, int joystick, int axis, float sensitivity, float deadZone);

    AxisConfiguration CreateRemoteAxis(string inputConfigName, string axisName);

    AxisConfiguration CreateRemoteButton(string inputConfigName, string buttonName);

    AxisConfiguration CreateAnalogButton(string inputConfigName, string buttonName, int joystick, int axis);

    /// <summary>
    /// Creates an uninitialized axis configuration. It's your responsability to configure the axis properly.
    /// </summary>
    AxisConfiguration CreateEmptyAxis(string inputConfigName, string axisName);

    bool DeleteAxisConfiguration(string inputConfigName, string axisName);

    /// <summary>
    /// Enables all of the input axis for all configurations
    /// </summary>
    void EnableAllAxes();

    /// <summary>
    /// Disables all of the input axis for the given configuration name
    /// </summary>
    /// <param name="configName">Name of configuration to disable all of the axes.</param>
    void DisableAllAxes();

    /// <summary>
    /// Enables a button in a input configuration
    /// </summary>
    /// <param name="configName">Name of the configuration to enable the axis on</param>
    /// <param name="buttonName">Name of the axis to enable</param>
    void EnableAxis(string buttonName);

    /// <summary>
    /// Disables a button in a input configuration
    /// </summary>
    /// <param name="configName">Name of the configuration to disable the axis on</param>
    /// <param name="buttonName">Name of the axis to disable</param>
    void DisableAxis(string buttonName);

    /// <summary>
    /// Scans for keyboard input and calls the handler with the result.
    /// Returns KeyCode.None if timeout is reached or the scan is canceled.
    /// </summary>
    [Obsolete("Use StartKeyboardButtonScan or StartJoystickButtonScan instead.")]
    void StartKeyScan(KeyScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData);

    /// <summary>
    /// Scans for keyboard input and calls the handler with the result.
    /// Returns KeyCode.None if timeout is reached or the scan is canceled.
    /// </summary>
    void StartKeyboardButtonScan(KeyScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData);

    /// <summary>
    /// Scans for mouse input and calls the handler with the result.
    /// Returns -1 if timeout is reached or the scan is canceled.
    /// </summary>
    void StartMouseAxisScan(AxisScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData);

    /// <summary>
    /// Scans for joystick button input and calls the handler with the result.
    /// Returns KeyCode.None if timeout is reached or the scan is canceled.
    /// <param name="joystick">The index of the joystick to scan for input. If null all joysticks will be scanned.</param>
    /// </summary>
    void StartJoystickButtonScan(KeyScanHandler scanHandler, int? joystick, float timeout, string cancelScanButton, params object[] userData);

    /// <summary>
    /// Scans for joystick input and calls the handler with the result.
    /// Returns -1 if timeout is reached or the scan is canceled.
    /// <param name="joystick">The index of the joystick to scan for input. If null all joysticks will be scanned.</param>
    /// </summary>
    void StartJoystickAxisScan(AxisScanHandler scanHandler, int? joystick, float timeout, string cancelScanButton, params object[] userData);

    void StartScan(ScanSettings settings, ScanHandler scanHandler);

    void CancelScan();

    /// <summary>
    /// Triggers the ConfigurationDirty event.
    /// </summary>
    void SetConfigurationDirty(string inputConfigName);

    /// <summary>
    /// Saves the input configurations in the XML format, in Application.persistentDataPath.
    /// </summary>
    void Save();

    /// <summary>
    /// Saves the input configurations in the XML format, at the specified location.
    /// </summary>
    void Save(string filename);
    void Save(IInputSaver inputSaver);

    /// <summary>
	/// Loads the input configurations saved in the XML format, from Application.persistentDataPath.
	/// </summary>
	void Load();
    void Load(string filename);

    void Load(IInputLoader inputLoader);
}
