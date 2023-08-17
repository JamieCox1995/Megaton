using System;
using UnityEngine;

namespace TotalDistraction.ServiceLocation.DefaultServices
{
    public class InputProxyService : IInputProxyService
    {
        public bool simulateMouseWithTouches { get { return Input.simulateMouseWithTouches; } set { Input.simulateMouseWithTouches = value; } }
        public bool anyKey { get { return Input.anyKey; } }
        public bool anyKeyDown { get { return Input.anyKeyDown; } }
        public string inputString { get { return Input.inputString; } }
        public Vector3 acceleration { get { return Input.acceleration; } }
        public AccelerationEvent[] accelerationEvents { get { return Input.accelerationEvents; } }
        public int accelerationEventCount { get { return Input.accelerationEventCount; } }
        public Touch[] touches { get { return Input.touches; } }
        public int touchCount { get { return Input.touchCount; } }
        public bool mousePresent { get { return Input.mousePresent; } }
        [Obsolete("eatKeyPressOnTextFieldFocus property is deprecated, and only provided to support legacy behavior.")]
        public bool eatKeyPressOnTextFieldFocus { get { return Input.eatKeyPressOnTextFieldFocus; } set { Input.eatKeyPressOnTextFieldFocus = value; } }
        public bool stylusTouchSupported { get { return Input.stylusTouchSupported; } }
        public bool touchSupported { get { return Input.touchSupported; } }
        public bool multiTouchEnabled { get { return Input.multiTouchEnabled; } set { Input.multiTouchEnabled = value; } }
        public LocationService location { get { return Input.location; } }
        public Compass compass { get { return Input.compass; } }
        public DeviceOrientation deviceOrientation { get { return Input.deviceOrientation; } }
        public IMECompositionMode imeCompositionMode { get { return Input.imeCompositionMode; } set { Input.imeCompositionMode = value; } }
        public string compositionString { get { return Input.compositionString; } }
        public bool imeIsSelected { get { return Input.imeIsSelected; } }
        public bool touchPressureSupported { get { return Input.touchPressureSupported; } }
        public Vector2 mouseScrollDelta { get { return Input.mouseScrollDelta; } }
        public Vector3 mousePosition { get { return Input.mousePosition; } }
        public Gyroscope gyro { get { return Input.gyro; } }
        public Vector2 compositionCursorPos { get { return Input.compositionCursorPos; } set { Input.compositionCursorPos = value; } }
        public bool backButtonLeavesApp { get { return Input.backButtonLeavesApp; } set { Input.backButtonLeavesApp = value; } }
        [Obsolete("isGyroAvailable property is deprecated. Please use SystemInfo.supportsGyroscope instead.")]
        public bool isGyroAvailable { get { return Input.isGyroAvailable; } }
        public bool compensateSensors { get { return Input.compensateSensors; } set { Input.compensateSensors = value; } }
        public AccelerationEvent GetAccelerationEvent(int index) { return Input.GetAccelerationEvent(index); }
        public float GetAxis(string axisName) { return Input.GetAxis(axisName); }
        public float GetAxisRaw(string axisName) { return Input.GetAxisRaw(axisName); }
        public bool GetButton(string buttonName) { return Input.GetButton(buttonName); }
        public bool GetButtonDown(string buttonName) { return Input.GetButtonDown(buttonName); }
        public bool GetButtonUp(string buttonName) { return Input.GetButtonUp(buttonName); }
        public string[] GetJoystickNames() { return Input.GetJoystickNames(); }
        public bool GetKey(string name) { return Input.GetKey(name); }
        public bool GetKey(KeyCode key) { return Input.GetKey(key); }
        public bool GetKeyDown(KeyCode key) { return Input.GetKeyDown(key); }
        public bool GetKeyDown(string name) { return Input.GetKeyDown(name); }
        public bool GetKeyUp(KeyCode key) { return Input.GetKeyUp(key); }
        public bool GetKeyUp(string name) { return Input.GetKeyUp(name); }
        public bool GetMouseButton(int button) { return Input.GetMouseButton(button); }
        public bool GetMouseButtonDown(int button) { return Input.GetMouseButtonDown(button); }
        public bool GetMouseButtonUp(int button) { return Input.GetMouseButtonUp(button); }
        public Touch GetTouch(int index) { return Input.GetTouch(index); }
#if UNITY_STANDALONE_LINUX
        public bool IsJoystickPreconfigured(string joystickName) { return Input.IsJoystickPreconfigured(joystickName); }
#endif
        public void ResetInputAxes() { Input.ResetInputAxes(); }
    }
}
