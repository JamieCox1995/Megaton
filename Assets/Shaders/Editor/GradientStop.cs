using System;
using UnityEngine;

[Serializable]
struct GradientStop : IComparable<GradientStop>
{
    public float t;
    public Color color;

    public int CompareTo(GradientStop other)
    {
        return t.CompareTo(other.t);
    }
}
