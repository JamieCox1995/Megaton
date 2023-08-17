using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Slider")]
public class OscilloscopeSlider : Slider, IOscilloscopeMaskable
{
    public PhosphorGraphics PhosphorGraphics;

    private RectTransform rectTransform;
    private Canvas canvas;

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        this.PhosphorGraphics = FindObjectOfType<PhosphorGraphics>();
    }
#endif

    protected override void Start()
    {
        base.Start();

        this.rectTransform = GetComponent<RectTransform>();
        this.canvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (this.IsMasked) this.PhosphorGraphics.BeginMask(this.MaskingRect, this.MaskingMode);
            Rect worldRect = this.rectTransform.GetWorldSpaceRect();
            Rect handleRect = this.handleRect.GetWorldSpaceRect();

            float t = Mathf.InverseLerp(this.minValue, this.maxValue, this.value);
            float halfSize = 0.5f * GetHandleSize(worldRect, handleRect);
            t = Mathf.Lerp(halfSize, 1 - halfSize, t);

            DrawSlideGrooveStop(0f, worldRect);
            DrawSlideGrooveStop(1f, worldRect);

            if (t > 0.001f)
            {
                // Draw the slide groove before the handle.
                DrawSlideGroove(0f, t - halfSize, worldRect);
            }

            if (t < 0.999f)
            {
                // Draw the slide groove after the handle.
                DrawSlideGroove(t + halfSize, 1f, worldRect);
            }

            if (this.wholeNumbers)
            {
                int min = Mathf.CeilToInt(this.minValue);
                int max = Mathf.FloorToInt(this.maxValue);

                for (int i = min + 1; i < max; i++)
                {
                    // Calculate the position of the slide groove stop.
                    float s = Mathf.InverseLerp(this.minValue, this.maxValue, i);
                    s = Mathf.Lerp(halfSize, 1 - halfSize, s);

                    // Draw the slide groove stop if not obscured by the handle.
                    if (s < t - halfSize || s > t + halfSize) DrawSlideGrooveStop(s, worldRect, 0.25f);
                }
            }
            if (this.IsMasked) this.PhosphorGraphics.EndMask();
        }
    }

    private void DrawSlideGroove(float tmin, float tmax, Rect rect)
    {
        rect = this.canvas.CanvasToOscilloscopeSpace(rect);

        Vector2 start, end;
        switch (this.direction)
        {
            case Direction.LeftToRight:
            case Direction.RightToLeft:

                float startX = Mathf.Lerp(rect.xMin, rect.xMax, tmin);
                float endX = Mathf.Lerp(rect.xMin, rect.xMax, tmax);

                start = new Vector2(startX, rect.center.y);
                end = new Vector2(endX, rect.center.y);
                break;

            case Direction.BottomToTop:
            case Direction.TopToBottom:

                float startY = Mathf.Lerp(rect.yMin, rect.yMax, tmin);
                float endY = Mathf.Lerp(rect.yMin, rect.yMax, tmax);

                start = new Vector2(rect.center.x, startY);
                end = new Vector2(rect.center.x, endY);
                break;

            default:
                throw new InvalidOperationException("Unknown slider direction");
        }

        this.PhosphorGraphics.DrawLine(start, end);
    }

    private void DrawSlideGrooveStop(float t, Rect rect, float height = 1f)
    {
        rect = this.canvas.CanvasToOscilloscopeSpace(rect);

        Vector2 start, end;
        switch (this.direction)
        {
            case Direction.LeftToRight:
            case Direction.RightToLeft:
                float tx = Mathf.Lerp(rect.xMin, rect.xMax, t);

                float yMin = Mathf.Lerp(rect.yMin, rect.yMax, (1f - height) / 2f);
                float yMax = Mathf.Lerp(rect.yMin, rect.yMax, (1f + height) / 2f);

                start = new Vector2(tx, yMin);
                end = new Vector2(tx, yMax);
                break;

            case Direction.BottomToTop:
            case Direction.TopToBottom:
                float ty = Mathf.Lerp(rect.yMin, rect.yMax, t);

                float xMin = Mathf.Lerp(rect.xMin, rect.xMax, (1f - height) / 2f);
                float xMax = Mathf.Lerp(rect.xMin, rect.xMax, (1f + height) / 2f);

                start = new Vector2(xMin, ty);
                end = new Vector2(xMax, ty);
                break;

            default:
                throw new InvalidOperationException("Unknown slider direction");
        }

        this.PhosphorGraphics.DrawLine(start, end);
    }

    private float GetHandleSize(Rect worldRect, Rect handleRect)
    {
        switch (this.direction)
        {
            case Direction.LeftToRight:
            case Direction.RightToLeft:
                return handleRect.width / worldRect.width;

            case Direction.BottomToTop:
            case Direction.TopToBottom:
                return handleRect.height / worldRect.height;

            default:
                throw new InvalidOperationException("Unknown slider direction");
        }
    }

    #region IOscilloscopeMaskable implementation
    private Rect? maskingRect;

    public MaskingMode MaskingMode { get; private set; }
    public Rect MaskingRect { get { return maskingRect ?? Rect.zero; } }
    public bool IsMasked { get { return maskingRect != null; } }

    public void SetMaskingRect(Rect rect)
    {
        maskingRect = rect;
    }

    public void SetMaskingMode(MaskingMode mode)
    {
        this.MaskingMode = mode;
    }

    public void ClearMaskingRect()
    {
        maskingRect = null;
    }
    #endregion
}
