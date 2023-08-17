using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static float ValidateAngle(float toCheck)
    {
        float angle = toCheck;

        if (angle < -360f)
        {
            angle += 360f;
        }

        if (angle > 360f)
        {
            angle -= 360f;
        }

        return angle;
    }
}
