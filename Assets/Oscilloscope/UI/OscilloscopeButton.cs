using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Button")]
public class OscilloscopeButton : Button, IOscilloscopeMaskable
{
    public PhosphorGraphics PhosphorGraphics;
    public float Thickness = 0.005f;

    private OscilloscopeText text;
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

        this.text = GetComponentInChildren<OscilloscopeText>();
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
