using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TeamUtility.IO;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public static class InputFormattingUtils
{
    /// <summary>
    /// Matches strings that contain substrings like "{axisName}", "{axis name}", "{axisName}", "{axisName.group}" and suchlike while ignoring padding whitespace.
    /// </summary>
    private static Regex AxisFormatRegex = new Regex(@"{\s*(?<AxisName>\S.*?)(\.(?<AxisProperty>group))?\s*}", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

    private const string UnknownInput = "<UNKNOWN INPUT>";

    public static string FormatInputAxes(this string str)
    {
        return AxisFormatRegex.Replace(str, ReplaceMatchWithControlName);
    }

    public static string FormatInputAxes(this string str, string format)
    {
        return AxisFormatRegex.Replace(str, match => ReplaceMatchWithFormattedControlName(match, format));
    }

    private static string ReplaceMatchWithControlName(Match match)
    {
        return ReplaceMatchWithFormattedControlName(match, null);
    }

    private static string ReplaceMatchWithFormattedControlName(Match match, string format)
    {
        string axisName = match.Groups["AxisName"].Value;

        AxisConfiguration config = ServiceLocator.Get<IInputManager>().GetAxisConfiguration(PlayerID.One, axisName);

        if (config == null) return UnknownInput;

        string axisProperty = match.Groups["AxisProperty"].Value;

        InputConfigurationNameMapping mapping = InputConfigurationNameMapping.GetMappingForConfiguration(ServiceLocator.Get<IInputManager>().PlayerOneConfiguration, config.type);

        if ("group".Equals(axisProperty, StringComparison.InvariantCultureIgnoreCase))
        {
            string groupName = mapping.GetAxisGroup(config.axis);
            return FormatDefault(format, groupName);
        }
        else if (string.IsNullOrEmpty(axisProperty))
        {
            return GetDefaultName(format, config, mapping);
        }
        else
        {
            return UnknownInput;
        }
    }

    private static string GetDefaultName(string format, AxisConfiguration config, InputConfigurationNameMapping mapping)
    {
        string result = string.Empty;

        switch (config.type)
        {
            case InputType.AnalogAxis:
            case InputType.RemoteAxis:
            case InputType.MouseAxis:
                string axisFriendlyName = mapping.GetAxisName(config.axis);
                result = FormatDefault(format, axisFriendlyName);
                break;

            case InputType.DigitalAxis:
                string positiveButtonName = mapping.GetButtonName(config.positive);
                string negativeButtonName = mapping.GetButtonName(config.negative);
                string combinedFormat = "{0} and {1}";
                string formattedPositive = FormatDefault(format, positiveButtonName);
                string formattedNegative = FormatDefault(format, negativeButtonName);
                result = string.Format(combinedFormat, formattedPositive, formattedNegative);
                break;

            case InputType.AnalogButton:
            case InputType.Button:
            case InputType.RemoteButton:
                string buttonName = mapping.GetButtonName(config.positive);
                result = FormatDefault(format, buttonName);
                break;
        }

        return result;
    }

    private static string FormatDefault(string format, string str)
    {
        return string.IsNullOrEmpty(format) ? str : string.Format(format, str);
    }

    private class InputConfigurationNameMapping
    {
        private Dictionary<int, string> _axisNames;
        private Dictionary<int, string> _axisGroups;
        private Dictionary<KeyCode, string> _buttonNames;

        protected InputConfigurationNameMapping(Dictionary<int, string> axisNames, Dictionary<int, string> axisGroups, Dictionary<KeyCode, string> buttonNames)
        {
            _axisNames = axisNames;
            _axisGroups = axisGroups;
            _buttonNames = buttonNames;
        }

        public static InputConfigurationNameMapping GetMappingForConfiguration(InputConfiguration inputConfig, InputType axisType)
        {
            switch (inputConfig.name)
            {
                case "Keyboard Default":
                    return axisType == InputType.MouseAxis ? (InputConfigurationNameMapping) new MouseConfigurationNameMapping() : (InputConfigurationNameMapping) new KeyboardConfigurationNameMapping();
                case "Xbox Default":
                    return new XboxConfigurationNameMapping();
                default:
                    return new DefaultConfigurationNameMapping();
            }
        }

        public string GetAxisName(int axis)
        {
            string result;

            if (!_axisNames.TryGetValue(axis, out result)) result = UnknownInput;

            return result;
        }

        public string GetAxisGroup(int axis)
        {
            string result;

            if (!_axisGroups.TryGetValue(axis, out result)) result = UnknownInput;

            return result;
        }

        public string GetButtonName(KeyCode keyCode)
        {
            string result;

            if (!_buttonNames.TryGetValue(keyCode, out result)) result = keyCode.ToString();

            return result;
        }

        private class DefaultConfigurationNameMapping : InputConfigurationNameMapping
        {
            public DefaultConfigurationNameMapping() : base(new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<KeyCode, string>())
            {
            }
        }

        private class MouseConfigurationNameMapping : InputConfigurationNameMapping
        {
            private static Dictionary<int, string> AxisNames = new Dictionary<int, string>()
        {
            { 0, "Vertical Mouse Movement" },
            { 1, "Horizontal Mouse Movement" },
            { 2, "Scroll Wheel" },
        };

            private static Dictionary<int, string> AxisGroups = new Dictionary<int, string>()
        {
            { 0, "Mouse Movement" },
            { 1, "Mouse Movement" },
            { 2, "Scroll Wheel" },
        };

            private static Dictionary<KeyCode, string> ButtonNames = new Dictionary<KeyCode, string>()
            {

            };

            public MouseConfigurationNameMapping() : base(AxisNames, AxisGroups, ButtonNames)
            {

            }
        }

        private class KeyboardConfigurationNameMapping : InputConfigurationNameMapping
        {
            private static Dictionary<int, string> AxisNames = new Dictionary<int, string>()
        {
            { 0, "Horizontal Arrow Keys" },
            { 1, "Vertical Arrow Keys" },
        };

            private static Dictionary<int, string> AxisGroups = new Dictionary<int, string>()
        {
            { 0, "Arrow Keys" },
            { 1, "Arrow Keys" },
        };

            private static Dictionary<KeyCode, string> ButtonNames = new Dictionary<KeyCode, string>()
        {
            { KeyCode.Mouse0, "Left Mouse Button" },
            { KeyCode.Mouse1, "Right Mouse Button" },
            { KeyCode.LeftArrow, "Left" },
            { KeyCode.RightArrow, "Right" },
            { KeyCode.UpArrow, "Up" },
            { KeyCode.DownArrow, "Down" },
            { KeyCode.LeftControl, "Left Ctrl" },
            { KeyCode.LeftShift, "Left Shift" },
        };

            public KeyboardConfigurationNameMapping() : base(AxisNames, AxisGroups, ButtonNames)
            {

            }
        }

        private class XboxConfigurationNameMapping : InputConfigurationNameMapping
        {
            private static Dictionary<int, string> AxisNames = new Dictionary<int, string>()
        {
            { 0, "LS Horizontal" },
            { 1, "LS Vertical" },
            { 2, "Triggers" },
            { 3, "RS Horizontal" },
            { 4, "RS Vertical" },
            { 5, "D-Pad Horizontal" },
            { 6, "D-Pad Vertical" },
        };

            private static Dictionary<int, string> AxisGroups = new Dictionary<int, string>()
        {
            { 0, "LS" },
            { 1, "LS" },
            { 2, "Triggers" },
            { 3, "RS" },
            { 4, "RS" },
            { 5, "D-Pad" },
            { 6, "D-Pad" },
        };

            private static Dictionary<KeyCode, string> ButtonNames = new Dictionary<KeyCode, string>()
        {
            { KeyCode.JoystickButton0, "A" },
            { KeyCode.JoystickButton1, "B" },
            { KeyCode.JoystickButton2, "X" },
            { KeyCode.JoystickButton3, "Y" },
            { KeyCode.JoystickButton4, "LB" },
            { KeyCode.JoystickButton5, "RB" },
            { KeyCode.JoystickButton6, "Back" },
            { KeyCode.JoystickButton7, "Start" },
            { KeyCode.JoystickButton8, "LS" },
            { KeyCode.JoystickButton9, "RS" },
        };

            public XboxConfigurationNameMapping() : base(AxisNames, AxisGroups, ButtonNames)
            {

            }
        }
    }
}

