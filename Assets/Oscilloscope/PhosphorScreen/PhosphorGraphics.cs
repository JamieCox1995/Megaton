using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Oscilloscope UI/Graphics Compositor")]
[RequireComponent(typeof(PhosphorScreenBuffer))]
public class PhosphorGraphics : MonoBehaviour
{
    public PhosphorScreenBuffer ScreenBuffer;

    public float MinimumLineDistance = 0.01f;

    private const int CurveResolution = 64;

    private MaskingMode maskingMode;
    private Rect? currentMaskingRect;

    private bool IsMasking { get { return currentMaskingRect != null; } }
    private Rect MaskingRect { get { return currentMaskingRect ?? Rect.zero; } }

    public bool InvertMask { get { return maskingMode == MaskingMode.MaskInside; } }

    private void Reset()
    {
        this.ScreenBuffer = GetComponent<PhosphorScreenBuffer>();
    }

    public void BeginMask(Rect rect, MaskingMode maskingMode)
    {
        if (maskingMode != MaskingMode.MaskOutside && maskingMode != MaskingMode.MaskInside) throw new ArgumentException("Invalid masking mode.");

        this.currentMaskingRect = rect;
        this.maskingMode = maskingMode;
    }

    public void EndMask()
    {
        this.currentMaskingRect = null;
        this.maskingMode = default(MaskingMode);
    }

    public void DrawLine(Vector2 start, Vector2 end)
    {
        if (this.IsMasking)
        {
            if (this.InvertMask)
            {
                Vector2 clipStart = start;
                Vector2 clipEnd = end;

                // If ClipLine() returns false, then the line is entirely outside the masked area, and should be drawn normally.
                if (this.MaskingRect.ClipLine(ref clipStart, ref clipEnd))
                {
                    // The line is at least partially within the masked area. Therefore, there are four possibilities:
                    //     clipStart == start && clipEnd == end -- the line is entirely within the masked area and should be discarded;
                    //     clipStart == start -- the end of the line should be drawn;
                    //     clipEnd == end -- the start of the line should be drawn;
                    //     or both the start and the end of the lines should be drawn, excluding the middle.

                    if (clipStart == start && clipEnd == end)
                    {
                        return;
                    }
                    else if (clipStart == start)
                    {
                        start = clipEnd;
                    }
                    else if (clipEnd == end)
                    {
                        end = clipStart;
                    }
                    else
                    {
                        DrawLineInternal(start, clipStart);
                        DrawLineInternal(clipEnd, end);
                        return;
                    }
                }
            }
            else if (!this.MaskingRect.ClipLine(ref start, ref end))
            {
                // The mask is not inverted, but the line is totally outside the masking rect. Discard it.
                return;
            }
        }

        // Draw the resulting line.
        DrawLineInternal(start, end);
    }

    private void DrawLineInternal(Vector2 start, Vector2 end)
    {
        this.ScreenBuffer.SetElectronBeamActive(false);
        this.ScreenBuffer.SetPosition(start);
        this.ScreenBuffer.SetElectronBeamActive(true);
        this.ScreenBuffer.SetPosition(end);
        this.ScreenBuffer.SetElectronBeamActive(false);
    }

    public void DrawPolyLine(params Vector2[] points)
    {
        if (points == null || points.Length < 2) throw new ArgumentException("points");

        if (points.Length == 2)
        {
            DrawLine(points[0], points[1]);
            return;
        }

        if (this.IsMasking)
        {
            // Split the polyline into separate clipped polylines.
            List<Vector2> sublinePoints = new List<Vector2>(points.Length);
            for (int i = 1; i < points.Length; i++)
            {
                Vector2 a = points[i - 1];
                Vector2 b = points[i];

                bool aInRect = this.MaskingRect.Contains(a);
                bool bInRect = this.MaskingRect.Contains(b);

                if (this.InvertMask)
                {
                    Vector2 clipA = a;
                    Vector2 clipB = b;

                    if (this.MaskingRect.ClipLine(ref clipA, ref clipB))
                    {
                        // The line is at least partially within the masked area. Therefore, there are four possibilities:
                        //     clipA == a && clipB == b -- the line is entirely within the masked area and should be discarded;
                        //     clipA == a -- the end of the line should be drawn;
                        //     clipB == b -- the start of the line should be drawn;
                        //     or both the start and the end of the lines should be drawn, excluding the middle.

                        if (clipA == a && clipB == b)
                        {
                            // The line segment is not visible, flush and draw the current buffer.
                            DrawPolyLineInternal(sublinePoints);
                            sublinePoints.Clear();
                        }
                        else if (clipA == a)
                        {
                            // Flush and draw the buffer and then add the end of the line segment to it.
                            DrawPolyLineInternal(sublinePoints);
                            sublinePoints.Clear();

                            sublinePoints.Add(clipB);
                            sublinePoints.Add(b);
                        }
                        else if (clipB == b)
                        {
                            // Add the start of the line segment to the buffer and then flush and draw it.
                            if (sublinePoints.Count == 0) sublinePoints.Add(a);
                            sublinePoints.Add(clipA);

                            DrawPolyLineInternal(sublinePoints);
                            sublinePoints.Clear();
                        }
                        else
                        {
                            DrawLineInternal(a, clipA);
                            DrawLineInternal(clipB, b);
                            return;
                        }
                    }
                    else
                    {
                        // Add the line segment to the buffer without modification.
                        if (sublinePoints.Count == 0) sublinePoints.Add(a);
                        sublinePoints.Add(b);
                    }
                }
                else
                {


                    if (aInRect && bInRect)
                    {
                        // Add the line segment to the buffer without modification.
                        if (sublinePoints.Count == 0) sublinePoints.Add(a);
                        sublinePoints.Add(b);
                    }
                    else if (aInRect)
                    {
                        if (this.MaskingRect.ClipLine(ref a, ref b))
                        {
                            // The line segment is partially visible, add the modified points to the buffer.
                            if (sublinePoints.Count == 0) sublinePoints.Add(a);
                            sublinePoints.Add(b);
                        }
                        else
                        {
                            // The line segment is not visible, flush and draw the current buffer.
                            DrawPolyLineInternal(sublinePoints);
                            sublinePoints.Clear();
                        }
                    }
                    else
                    {
                        // The line segment either ends within the rect, traverses it, or is completely outside.
                        // Either way, if the line can be clipped, both points need to be added to the buffer.
                        if (this.MaskingRect.ClipLine(ref a, ref b))
                        {
                            sublinePoints.Add(a);
                            sublinePoints.Add(b);
                        }
                        else
                        {
                            // The line segment is not visible, flush and draw the current buffer.
                            DrawPolyLineInternal(sublinePoints);
                            sublinePoints.Clear();
                        }
                    }
                }
            }

            // Draw any remaining points.
            DrawPolyLineInternal(sublinePoints);
        }
        else
        {
            DrawPolyLineInternal(points);
        }
    }

    private void DrawPolyLineInternal(IList<Vector2> points)
    {
        this.ScreenBuffer.SetElectronBeamActive(false);

        if (points.Count == 0) return;

        float minSqDistance = this.MinimumLineDistance * this.MinimumLineDistance;

        Vector2 previous = points[0];
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 current = points[i];

            if (i > 0)
            {
                float sqDistance = (previous - current).sqrMagnitude;

                if (sqDistance < minSqDistance) continue;
            }


            this.ScreenBuffer.SetPosition(points[i]);

            if (i == 0) this.ScreenBuffer.SetElectronBeamActive(true);

            previous = points[i];
        }

        this.ScreenBuffer.SetElectronBeamActive(false);
    }

    public void DrawRect(Rect rect)
    {
        DrawPolyLine(rect.min, new Vector2(rect.xMin, rect.yMax), rect.max, new Vector2(rect.xMax, rect.yMin), rect.min);
    }

    public void DrawRoundRect(Rect rect, float cornerRadius)
    {
        if (cornerRadius == 0) DrawRect(rect);

        float xMin = rect.xMin;
        float xMinInner = rect.xMin + cornerRadius;
        float xMax = rect.xMax;
        float xMaxInner = rect.xMax - cornerRadius;

        float yMin = rect.yMin;
        float yMinInner = rect.yMin + cornerRadius;
        float yMax = rect.yMax;
        float yMaxInner = rect.yMax - cornerRadius;

        DrawLine(new Vector2(xMin, yMinInner), new Vector2(xMin, yMaxInner));
        DrawArc(new Vector2(xMinInner, yMaxInner), cornerRadius, 0.75f, 1f);
        DrawLine(new Vector2(xMinInner, yMax), new Vector2(xMaxInner, yMax));
        DrawArc(new Vector2(xMaxInner, yMaxInner), cornerRadius, 0f, 0.25f);
        DrawLine(new Vector2(xMax, yMaxInner), new Vector2(xMax, yMinInner));
        DrawArc(new Vector2(xMaxInner, yMinInner), cornerRadius, 0.25f, 0.5f);
        DrawLine(new Vector2(xMaxInner, yMin), new Vector2(xMinInner, yMin));
        DrawArc(new Vector2(xMinInner, yMinInner), cornerRadius, 0.5f, 0.75f);
    }

    public void DrawCircle(Vector2 center, float radius)
    {
        Vector2[] points = new Vector2[CurveResolution + 1];
        for (int i = 0; i <= CurveResolution; i++)
        {
            float t = (2f * Mathf.PI * i) / CurveResolution;

            points[i].x = center.x + radius * Mathf.Sin(t);
            points[i].y = center.y + radius * Mathf.Cos(t);
        }

        DrawPolyLine(points);
    }

    public void DrawEllipse(Vector2 center, float major, float minor, float rotation)
    {
        float theta = Mathf.Deg2Rad * rotation;

        Vector2[] points = new Vector2[CurveResolution + 1];
        for (int i = 0; i <= CurveResolution; i++)
        {
            float t = (2f * Mathf.PI * i) / CurveResolution;

            float cosTheta = Mathf.Cos(theta);
            float sinTheta = Mathf.Sin(theta);

            float x = major * Mathf.Sin(t);
            float y = minor * Mathf.Cos(t);

            float u = x * cosTheta - y * sinTheta;
            float v = x * sinTheta + y * cosTheta;

            points[i].x = center.x + u;
            points[i].y = center.y + v;
        }

        DrawPolyLine(points);
    }

    public void DrawArc(Vector2 center, float radius, float a, float b)
    {
        if (b - a > 2f * Mathf.PI)
        {
            a = 0f;
            b = 1f;
        }

        while (a > b)
        {
            b += Mathf.PI * 2f;
        }

        float range = b - a;

        int resolution = Mathf.CeilToInt(CurveResolution * range);

        Vector2[] points = new Vector2[resolution + 1];
        for (int i = 0; i <= resolution; i++)
        {
            float t = 2f * Mathf.PI * Mathf.Lerp(a, b, (float)i / resolution);

            points[i].x = center.x + radius * Mathf.Sin(t);
            points[i].y = center.y + radius * Mathf.Cos(t);
        }

        DrawPolyLine(points);
    }

    public void DrawArc(Vector2 center, float major, float minor, float rotation, float a, float b)
    {
        if (b - a > 2f * Mathf.PI)
        {
            a = 0f;
            b = 1f;
        }

        while (a > b)
        {
            b += Mathf.PI * 2f;
        }

        float range = b - a;

        int resolution = Mathf.CeilToInt(CurveResolution * range);

        float theta = Mathf.Deg2Rad * rotation;

        Vector2[] points = new Vector2[resolution + 1];
        for (int i = 0; i <= resolution; i++)
        {
            float t = 2f * Mathf.PI * Mathf.Lerp(a, b, (float)i / resolution);

            float cosTheta = Mathf.Cos(theta);
            float sinTheta = Mathf.Sin(theta);

            float x = major * Mathf.Sin(t);
            float y = minor * Mathf.Cos(t);

            float u = x * cosTheta - y * sinTheta;
            float v = x * sinTheta + y * cosTheta;

            points[i].x = center.x + u;
            points[i].y = center.y + v;
        }

        DrawPolyLine(points);
    }

    public void Plot(Rect plotArea, Rect plotRange, Func<float, float> func, PlotOptions plotOptions = PlotOptions.None)
    {
        if (plotOptions != PlotOptions.None)
        {
            DrawPlotBorder(plotArea, plotOptions);
            if (plotOptions.HasFlag(PlotOptions.XAxis)) DrawXAxis(plotArea, plotRange, plotOptions);
            if (plotOptions.HasFlag(PlotOptions.YAxis)) DrawYAxis(plotArea, plotRange, plotOptions);
        }

        Vector2[] points = new Vector2[CurveResolution];
        for (int i = 0; i < CurveResolution; i++)
        {
            float t = (float)i / (CurveResolution - 1);

            float x = Mathf.Lerp(plotRange.xMin, plotRange.xMax, t);
            float y = func(x);

            y = Mathf.Clamp(y, plotRange.yMin, plotRange.yMax);

            float plotX = t;
            float plotY = Mathf.InverseLerp(plotRange.yMin, plotRange.yMax, y);

            float screenX = Mathf.Lerp(plotArea.xMin, plotArea.xMax, plotX);
            float screenY = Mathf.Lerp(plotArea.yMin, plotArea.yMax, plotY);

            points[i].x = screenX;
            points[i].y = screenY;
        }

        DrawPolyLine(points);
    }

    public void Plot(Rect plotArea, Rect plotRange, Func<float, float> xFunc, Func<float, float> yFunc, float tMin, float tMax, PlotOptions plotOptions = PlotOptions.None)
    {
        if (plotOptions != PlotOptions.None)
        {
            DrawPlotBorder(plotArea, plotOptions);
            if (plotOptions.HasFlag(PlotOptions.XAxis)) DrawXAxis(plotArea, plotRange, plotOptions);
            if (plotOptions.HasFlag(PlotOptions.YAxis)) DrawYAxis(plotArea, plotRange, plotOptions);
        }

        Vector2[] points = new Vector2[CurveResolution];
        for (int i = 0; i < CurveResolution; i++)
        {
            float s = (float)i / (CurveResolution - 1);
            float t = Mathf.Lerp(tMin, tMax, s);

            float x = xFunc(t);
            float y = yFunc(t);

            x = Mathf.Clamp(x, plotRange.xMin, plotRange.xMax);
            y = Mathf.Clamp(y, plotRange.yMin, plotRange.yMax);

            float plotX = Mathf.InverseLerp(plotRange.xMin, plotRange.xMax, x);
            float plotY = Mathf.InverseLerp(plotRange.yMin, plotRange.yMax, y);

            float screenX = Mathf.Lerp(plotArea.xMin, plotArea.xMax, plotX);
            float screenY = Mathf.Lerp(plotArea.yMin, plotArea.yMax, plotY);

            points[i].x = screenX;
            points[i].y = screenY;
        }

        DrawPolyLine(points);
    }

    private void DrawPlotBorder(Rect plotArea, PlotOptions plotOptions)
    {
        if (plotOptions.HasFlag(PlotOptions.FullBorder))
        {
            DrawRect(plotArea);
        }
        else
        {
            if (plotOptions.HasFlag(PlotOptions.BorderLeft)) DrawLine(plotArea.min, new Vector2(plotArea.xMin, plotArea.yMax));
            if (plotOptions.HasFlag(PlotOptions.BorderRight)) DrawLine(new Vector2(plotArea.xMax, plotArea.yMin), plotArea.max);
            if (plotOptions.HasFlag(PlotOptions.BorderBottom)) DrawLine(plotArea.min, new Vector2(plotArea.xMax, plotArea.yMin));
            if (plotOptions.HasFlag(PlotOptions.BorderTop)) DrawLine(new Vector2(plotArea.xMin, plotArea.yMax), plotArea.max);
        }
    }

    private void DrawXAxis(Rect plotArea, Rect plotRange, PlotOptions plotOptions)
    {
        float axisY = Mathf.InverseLerp(plotRange.yMin, plotRange.yMax, 0f);

        bool axisIsBottomBorder = axisY == 0 && plotOptions.HasFlag(PlotOptions.BorderBottom);
        bool axisIsTopBorder = axisY == 1 && plotOptions.HasFlag(PlotOptions.BorderTop);

        if (!axisIsTopBorder && !axisIsBottomBorder)
        {
            float screenY = Mathf.Lerp(plotArea.yMin, plotArea.yMax, axisY);

            DrawLine(new Vector2(plotArea.xMin, screenY), new Vector2(plotArea.xMax, screenY));
        }
    }

    private void DrawYAxis(Rect plotArea, Rect plotRange, PlotOptions plotOptions)
    {
        float axisX = Mathf.InverseLerp(plotRange.xMin, plotRange.xMax, 0f);

        bool axisIsLeftBorder = axisX == 0 && plotOptions.HasFlag(PlotOptions.BorderLeft);
        bool axisIsRightBorder = axisX == 1 && plotOptions.HasFlag(PlotOptions.BorderRight);

        if (!axisIsRightBorder && !axisIsLeftBorder)
        {
            float screenX = Mathf.Lerp(plotArea.xMin, plotArea.xMax, axisX);

            DrawLine(new Vector2(screenX, plotArea.yMin), new Vector2(screenX, plotArea.yMax));
        }
    }
}

[Flags]
public enum PlotOptions
{
    None = 0x00,
    FullBorder = BorderLeft | BorderRight | BorderTop | BorderBottom,
    BothAxes = XAxis | YAxis,
    All = FullBorder | BothAxes,

    BorderLeft = 0x01,
    BorderRight = 0x02,
    BorderBottom = 0x04,
    BorderTop = 0x08,

    XAxis = 0x10,
    YAxis = 0x20,
}

public static class PlotOptionsHelper
{
    public static bool HasFlag(this PlotOptions options, PlotOptions flag)
    {
        return (options & flag) == flag;
    }
}