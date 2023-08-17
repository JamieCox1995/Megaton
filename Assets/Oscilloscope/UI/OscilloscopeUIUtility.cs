using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OscilloscopeUIUtility
{
    public static Rect GetWorldSpaceRect(this RectTransform rectTransform)
    {
        Rect localRect = rectTransform.rect;
        localRect.min = rectTransform.TransformPoint(localRect.min);
        localRect.max = rectTransform.TransformPoint(localRect.max);

        return localRect;
    }

    public static Rect CanvasToOscilloscopeSpace(this Canvas canvas, Rect rect)
    {
        RectTransform rectTransform = canvas.GetComponent<RectTransform>();

        Vector3[] localCorners = new Vector3[4];
        rectTransform.GetWorldCorners(localCorners);
        
        Vector2 bottomLeft = localCorners[0];
        Vector2 topRight = localCorners[2];

        Vector2 dimensions = topRight - bottomLeft;

        float aspectRatio = dimensions.x / dimensions.y;

        float xMin = aspectRatio * (2f * Mathf.InverseLerp(bottomLeft.x, topRight.x, rect.xMin) - 1f);
        float xMax = aspectRatio * (2f * Mathf.InverseLerp(bottomLeft.x, topRight.x, rect.xMax) - 1f);
        float yMin = 2f * Mathf.InverseLerp(bottomLeft.y, topRight.y, rect.yMin) - 1f;
        float yMax = 2f * Mathf.InverseLerp(bottomLeft.y, topRight.y, rect.yMax) - 1f;

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
}
