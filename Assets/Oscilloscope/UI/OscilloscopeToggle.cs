using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Toggle")]
public class OscilloscopeToggle : Toggle, IOscilloscopeMaskable
{
    public PhosphorGraphics PhosphorGraphics;
    public float Thickness = 0.005f;

    private RectTransform rectTransform;

    private Canvas canvas;

    private SelectionState selectionState;

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
        Rect worldRect = this.rectTransform.GetWorldSpaceRect();

        if (this.IsMasked) this.PhosphorGraphics.BeginMask(this.MaskingRect, this.MaskingMode);
        switch (this.selectionState)
        {
            case SelectionState.Disabled:
            case SelectionState.Normal:
                break;

            case SelectionState.Highlighted:
                Rect rect = this.canvas.CanvasToOscilloscopeSpace(worldRect);

                this.PhosphorGraphics.DrawRect(rect);
                break;

            case SelectionState.Pressed:
                Rect outer = this.canvas.CanvasToOscilloscopeSpace(worldRect);
                this.PhosphorGraphics.DrawRect(outer);

                Rect inner = Rect.MinMaxRect(outer.xMin + this.Thickness, outer.yMin + this.Thickness, outer.xMax - this.Thickness, outer.yMax - this.Thickness);

                this.PhosphorGraphics.DrawRect(inner);
                break;
        }
        if (this.IsMasked) this.PhosphorGraphics.EndMask();

        this.graphic.enabled = this.isOn;
    }

    private void DrawDot(Vector2 center, float radius)
    {
        this.PhosphorGraphics.DrawCircle(center, radius);
    }

    private void DrawSquareDot(Vector2 center, float radius)
    {
        Rect dotRect = Rect.MinMaxRect(center.x - radius, center.y - radius, center.x + radius, center.y + radius);

        this.PhosphorGraphics.DrawRect(dotRect);
    }

    private void DrawCross(Vector2 center, float radius)
    {
        Rect crossRect = Rect.MinMaxRect(center.x - radius, center.y - radius, center.x + radius, center.y + radius);

        this.PhosphorGraphics.DrawLine(crossRect.min, crossRect.max);

        this.PhosphorGraphics.DrawLine(new Vector2(crossRect.xMax, crossRect.yMin), new Vector2(crossRect.xMin, crossRect.yMax));
    }

    private void DrawCheck(Vector2 center, float radius)
    {
        Rect checkRect = Rect.MinMaxRect(center.x - radius, center.y - radius, center.x + radius, center.y + radius);

        Vector2[] points = new[]
        {
            new Vector2(checkRect.xMin, checkRect.center.y),
            new Vector2(Mathf.Lerp(checkRect.xMin, checkRect.xMax, 0.4f), checkRect.yMin),
            checkRect.max
        };

        this.PhosphorGraphics.DrawPolyLine(points);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        this.selectionState = state;
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
