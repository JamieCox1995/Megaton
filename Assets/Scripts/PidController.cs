using UnityEngine;

[System.Serializable]
public class PidController
{
    public float ProportionalGain;
    public float IntegralGain;
    public float DerivativeGain;

    [Space()]
    public float MaxError = 20f;

#if UNITY_EDITOR
    public bool EmitDebugOutput;
#endif

    private float _previousError;
    private float _totalError;

    public Vector3 GainFactors
    {
        get
        {
            return new Vector3(this.ProportionalGain, this.IntegralGain, this.DerivativeGain);
        }

        set
        {
            this.ProportionalGain = value.x;
            this.IntegralGain = value.y;
            this.DerivativeGain = value.z;
        }
    }

    public void Reset()
    {
        _previousError = 0;
        _totalError = 0;
    }

    public float GetOutput(float error, float deltaTime)
    {
        _totalError += error * deltaTime;
        float errorDerivative = (error - _previousError) / deltaTime;

        _previousError = error;

        _totalError = Mathf.Clamp(_totalError, -this.MaxError, this.MaxError);

#if UNITY_EDITOR
        if (this.EmitDebugOutput) Debug.LogFormat("P = {0:F3}, I = {1:F3}, D = {2:F3}", error, _totalError, errorDerivative);
#endif

        return error * this.ProportionalGain + _totalError * this.IntegralGain + errorDerivative * this.DerivativeGain;
    }
}

[System.Serializable]
public class PidController3
{
    public Vector3 ProportionalGain;
    public Vector3 IntegralGain;
    public Vector3 DerivativeGain;

    [Space()]
    public float MaxError = 20f;

#if UNITY_EDITOR
    public bool EmitDebugOutput;
#endif

    private Vector3 _previousError;
    private Vector3 _totalError;


    public void Reset()
    {
        _previousError = Vector3.zero;
        _totalError = Vector3.zero;
    }

    public Vector3 GetOutput(Vector3 error, float deltaTime)
    {
        _totalError += error * deltaTime;
        Vector3 errorDerivative = (error - _previousError) / deltaTime;

        _previousError = error;

        _totalError = ClampComponentwise(_totalError, -this.MaxError, this.MaxError);

#if UNITY_EDITOR
        if (this.EmitDebugOutput) Debug.LogFormat("P = {0:F3}, I = {1:F3}, D = {2:F3}", error, _totalError, errorDerivative);
#endif

        return Vector3.Scale(error, this.ProportionalGain) + Vector3.Scale(_totalError, this.IntegralGain) + Vector3.Scale(errorDerivative, this.DerivativeGain);
    }

    private Vector3 ClampComponentwise(Vector3 value, float min, float max)
    {
        return new Vector3(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max), Mathf.Clamp(value.z, min, max));
    }
}
