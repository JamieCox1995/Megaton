using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Scrollbar")]
public class OscilloscopeScrollbar : Scrollbar, IOscilloscopeMaskable
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
            float maxValue = 1f - this.size;

            float tmin = Mathf.Lerp(0, maxValue, this.value);
            float tmax = tmin + this.size;

            Rect worldRect = this.rectTransform.GetWorldSpaceRect();

            DrawHandle(tmin, tmax, worldRect);

            if (tmin > 0.001f)
            {
                DrawSlideGroove(0f, tmin, worldRect);
                DrawSlideGrooveStop(0f, worldRect);
            }

            if (tmax < 0.999f)
            {
                DrawSlideGroove(tmax, 1f, worldRect);
                DrawSlideGrooveStop(1f, worldRect);
            }
            if (this.IsMasked) this.PhosphorGraphics.EndMask();
        }
    }

    private void DrawHandle(float tmin, float tmax, Rect rect)
    {
        float xmin, xmax, ymin, ymax;
        switch (this.direction)
        {
            case Direction.LeftToRight:
            case Direction.RightToLeft:
                xmin = Mathf.Lerp(rect.xMin, rect.xMax, tmin);
                xmax = Mathf.Lerp(rect.xMin, rect.xMax, tmax);
                ymin = rect.yMin;
                ymax = rect.yMax;
                break;

            case Direction.BottomToTop:
            case Direction.TopToBottom:
                xmin = rect.xMin;
                xmax = rect.xMax;
                ymin = Mathf.Lerp(rect.yMin, rect.yMax, tmin);
                ymax = Mathf.Lerp(rect.yMin, rect.yMax, tmax);
                break;

            default:
                throw new InvalidOperationException("Unknown scrollbar direction");
        }

        Rect handleRect = Rect.MinMaxRect(xmin, ymin, xmax, ymax);

        this.PhosphorGraphics.DrawRect(this.canvas.CanvasToOscilloscopeSpace(handleRect));
    }

    private void DrawSlideGroove(float tmin, float tmax, Rect rect)
    {
        rect = this.canvas.CanvasToOscilloscopeSpace(rect);

        Vector2 start, end;
        switch (this.direction)
        {
            case Direction.LeftToRight:
            case Direction.RightToLeft:
                start = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, tmin), rect.center.y);
                end = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, tmax), rect.center.y);
                break;

            case Direction.BottomToTop:
            case Direction.TopToBottom:
                start = new Vector2(rect.center.x, Mathf.Lerp(rect.yMin, rect.yMax, tmin));
                end = new Vector2(rect.center.x, Mathf.Lerp(rect.yMin, rect.yMax, tmax));
                break;

            default:
                throw new InvalidOperationException("Unknown scrollbar direction");
        }

        this.PhosphorGraphics.DrawLine(start, end);
    }

    private void DrawSlideGrooveStop(float t, Rect rect)
    {
        rect = this.canvas.CanvasToOscilloscopeSpace(rect);

        Vector2 start, end;
        switch (this.direction)
        {
            case Direction.LeftToRight:
            case Direction.RightToLeft:
                start = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, t), rect.yMin);
                end = new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, t), rect.yMax);
                break;

            case Direction.BottomToTop:
            case Direction.TopToBottom:
                start = new Vector2(rect.xMin, Mathf.Lerp(rect.yMin, rect.yMax, t));
                end = new Vector2(rect.xMax, Mathf.Lerp(rect.yMin, rect.yMax, t));
                break;

            default:
                throw new InvalidOperationException("Unknown scrollbar direction");
        }

        this.PhosphorGraphics.DrawLine(start, end);
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
