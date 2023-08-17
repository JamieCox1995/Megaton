using UnityEngine;

namespace TotalDistraction.ServiceLocation.DefaultServices
{
    public interface IInputProxyService : IUnityService
    {
        bool simulateMouseWithTouches { get; set; }
        bool anyKey { get; }
        bool anyKeyDown { get; }
        string inputString { get; }
        Vector3 acceleration { get; }
        AccelerationEvent[] accelerationEvents { get; }
        int accelerationEventCount { get; }
        Touch[] touches { get; }
        int touchCount { get; }
        bool mousePresent { get; }
        bool eatKeyPressOnTextFieldFocus { get; set; }
        bool stylusTouchSupported { get; }
        bool touchSupported { get; }
        bool multiTouchEnabled { get; set; }
        LocationService location { get; }
        Compass compass { get; }
        DeviceOrientation deviceOrientation { get; }
        IMECompositionMode imeCompositionMode { get; set; }
        string compositionString { get; }
        bool imeIsSelected { get; }
        bool touchPressureSupported { get; }
        Vector2 mouseScrollDelta { get; }
        Vector3 mousePosition { get; }
        Gyroscope gyro { get; }
        Vector2 compositionCursorPos { get; set; }
        bool backButtonLeavesApp { get; set; }
        bool isGyroAvailable { get; }
        bool compensateSensors { get; set; }
        AccelerationEvent GetAccelerationEvent(int index);
        float GetAxis(string axisName);
        float GetAxisRaw(string axisName);
        bool GetButton(string buttonName);
        bool GetButtonDown(string buttonName);
        bool GetButtonUp(string buttonName);
        string[] GetJoystickNames();
        bool GetKey(string name);
        bool GetKey(KeyCode key);
        bool GetKeyDown(KeyCode key);
        bool GetKeyDown(string name);
        bool GetKeyUp(KeyCode key);
        bool GetKeyUp(string name);
        bool GetMouseButton(int button);
        bool GetMouseButtonDown(int button);
        bool GetMouseButtonUp(int button);
        Touch GetTouch(int index);
#if UNITY_STANDALONE_LINUX
        bool IsJoystickPreconfigured(string joystickName);
#endif
        void ResetInputAxes();
    }
}
