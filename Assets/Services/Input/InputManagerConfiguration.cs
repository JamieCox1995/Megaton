using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.ServiceLocation;

[CreateAssetMenu(menuName = "Service Configuration/Input Manager Configuration", order = 1)]
public class InputManagerConfiguration : ServiceConfiguration
{
    public string KeyboardConfiguration;
    public string WindowsJoystickConfiguration;
    public string OsxJoystickConfiguration;
    public string LinuxJoystickConfiguration;
    public string LeftTriggerAxis;
    public string RightTriggerAxis;
    public string DpadHorizontalAxis;
    public string DpadVerticalAxis;
}