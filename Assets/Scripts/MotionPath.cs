using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MotionPath : MonoBehaviour, ISerializationCallbackReceiver
{
    public string Name;
    public List<ControlPoint> ControlPoints;
    public bool IsClosedPath;

    public float Length { get { return _totalLength; } }

    private Dictionary<float, float> _lookup;
    [SerializeField, HideInInspector]
    private float _totalLength;

    [SerializeField, HideInInspector]
    private List<float> _lookupKeys;
    [SerializeField, HideInInspector]
    private List<float> _lookupValues;

    private static readonly Func<float, float>[,] HermiteBasis = new Func<float, float>[,]
    {
        {
            t => 2 * Mathf.Pow(t, 3) - 3 * Mathf.Pow(t, 2) + 1,
            t => -2 * Mathf.Pow(t, 3) + 3 * Mathf.Pow(t, 2),
        },
        {
            t => Mathf.Pow(t, 3) - 2 * Mathf.Pow(t, 2) + t,
            t => Mathf.Pow(t, 3) - Mathf.Pow(t, 2),
        },
    };

    private const int InterpolationDivisions = 256;

    private void OnValidate()
    {
        _lookup = new Dictionary<float, float>();
        List<Vector3> points = GetInterpolatedPoints(InterpolationDivisions);

        float length = 0f;
        _lookup.Add(0f, 0f);
        for (int i = 1; i < points.Count; i++)
        {
            float t = (float)i / (float)InterpolationDivisions;

            length += (points[i] - points[i - 1]).magnitude;
            _lookup.Add(length, t);
        }
        _totalLength = length;
    }

    public void OnBeforeSerialize()
    {
        if (_lookup != null)
        {
            _lookupKeys = new List<float>(_lookup.Keys);
            _lookupValues = new List<float>(_lookup.Values);
        }
    }

    public void OnAfterDeserialize()
    {
        if (_lookupKeys != null && _lookupValues != null)
        {
            _lookup = new Dictionary<float, float>();
            int count = Mathf.Min(_lookupKeys.Count, _lookupValues.Count);

            for (int i = 0; i < count; i++)
            {
                _lookup.Add(_lookupKeys[i], _lookupValues[i]);
            }
        }
    }

    public Vector3 Evaluate(float t)
    {
        if (this.ControlPoints == null || this.ControlPoints.Count == 0) throw new InvalidOperationException();

        List<ControlPoint> controlPoints = new List<ControlPoint>(this.ControlPoints);

        if (this.IsClosedPath) controlPoints.Add(this.ControlPoints[0]);

        if (t <= 0) return controlPoints.First().Point;
        if (t >= 1) return controlPoints.Last().Point;

        float length = t * _totalLength;

        var interval = IntervalHelper.FindInterval(_lookup, length);

        float s;

        switch (interval.Type)
        {
            case IntervalHelper.IntervalType.UnboundedLeft:
            case IntervalHelper.IntervalType.UnboundedRight:
            default:
                throw new InvalidOperationException();

            case IntervalHelper.IntervalType.Bounded:
                float lowerLength = interval.LowerBound.Key;
                float upperLength = interval.UpperBound.Key;

                var parameterValue = Mathf.InverseLerp(lowerLength, upperLength, length);

                float lowerT = interval.LowerBound.Value;
                float upperT = interval.UpperBound.Value;

                s = Mathf.Lerp(lowerT, upperT, parameterValue);
                break;

            case IntervalHelper.IntervalType.Exact:
                s = interval.LowerBound.Value;
                break;
        }

        int controlPointIndex = Mathf.FloorToInt(s);
        s = s % 1;

        ControlPoint a = controlPoints[controlPointIndex];
        ControlPoint b = controlPoints[controlPointIndex + 1];

        return InterpolatePoint(a, b, s);
    }

    public List<Vector3> GetInterpolatedPoints(int divisions, bool interpolateStraightLines = true)
    {
        if (this.ControlPoints == null || this.ControlPoints.Count == 0) return new List<Vector3>();

        List<Vector3> result = new List<Vector3>();

        List<ControlPoint> controlPoints = new List<ControlPoint>(this.ControlPoints);

        if (this.IsClosedPath) controlPoints.Add(controlPoints[0]);

        for (int i = 1; i < controlPoints.Count; i++)
        {
            ControlPoint previous = controlPoints[i - 1];
            ControlPoint current = controlPoints[i];

            if (!interpolateStraightLines && previous.Tangent == Vector3.zero && current.Tangent == Vector3.zero)
            {
                if (result.Count == 0 || result[result.Count - 1] != previous.Point) result.Add(previous.Point);
                result.Add(current.Point);

                continue;
            }

            for (int j = 0; j < divisions; j++)
            {
                float t = (float)j / (float)divisions;

                Vector3 pt = InterpolatePoint(previous, current, t);

                result.Add(pt);
            }
        }

        if (!this.IsClosedPath) result.Add(controlPoints[controlPoints.Count - 1].Point);

        return result;
    }

    private static Vector3 InterpolatePoint(ControlPoint a, ControlPoint b, float t)
    {
        Vector3 p0 = a.Point;
        Vector3 p1 = b.Point;
        Vector3 m0 = a.Tangent;
        Vector3 m1 = b.Tangent;

        Vector3 pt = HermiteBasis[0, 0](t) * p0 + HermiteBasis[1, 0](t) * m0 + HermiteBasis[0, 1](t) * p1 + HermiteBasis[1, 1](t) * m1;
        return pt;
    }

    [Serializable]
    public class ControlPoint
    {
        public Vector3 Point;
        public Vector3 Tangent;
    }

    private static class IntervalHelper
    {
        public static IntervalHelperResult<TKey, TValue> FindInterval<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key) where TKey : IComparable<TKey>
        {
            TKey[] keys = dict.Keys.OrderBy(k => k).ToArray();

            int compareToFirst = key.CompareTo(keys[0]);

            if (compareToFirst == -1)
            {
                return new IntervalHelperResult<TKey, TValue>
                {
                    Type = IntervalType.UnboundedLeft,
                    UpperBound = dict.First(kvp => kvp.Key.Equals(keys[0]))
                };
            }
            else if (compareToFirst == 0)
            {
                var bound = dict.First(kvp => kvp.Key.Equals(keys[0]));
                return new IntervalHelperResult<TKey, TValue>
                {
                    Type = IntervalType.Exact,
                    LowerBound = bound,
                    UpperBound = bound,
                };
            }

            for (int i = 1; i < keys.Length; i++)
            {
                int comparison = key.CompareTo(keys[i]);

                if (comparison == 1)
                {
                    continue;
                }
                else if (comparison == 0)
                {
                    var bound = dict.First(kvp => kvp.Key.Equals(keys[i]));
                    return new IntervalHelperResult<TKey, TValue>
                    {
                        Type = IntervalType.Exact,
                        LowerBound = bound,
                        UpperBound = bound,
                    };
                }
                else if (comparison == -1)
                {
                    return new IntervalHelperResult<TKey, TValue>
                    {
                        Type = IntervalType.Bounded,
                        LowerBound = dict.First(kvp => kvp.Key.Equals(keys[i - 1])),
                        UpperBound = dict.First(kvp => kvp.Key.Equals(keys[i])),
                    };
                }
            }

            return new IntervalHelperResult<TKey, TValue>
            {
                Type = IntervalType.UnboundedRight,
                LowerBound = dict.First(kvp => kvp.Key.Equals(keys.Last())),
            };
        }

        public class IntervalHelperResult<TKey, TValue>
        {
            public IntervalType Type;
            public KeyValuePair<TKey, TValue> LowerBound;
            public KeyValuePair<TKey, TValue> UpperBound;
        }

        public enum IntervalType
        {
            Exact,
            Bounded,
            UnboundedLeft,
            UnboundedRight,
        }
    }
}
