using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Panel")]
public class OscilloscopePanel : Graphic, IOscilloscopeMaskable
{
    public PhosphorGraphics PhosphorGraphics;
    public BorderStyle BorderStyle;
    public float CornerRadius;

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
    }

    private void Update()
    {
        if (Application.isPlaying && this.BorderStyle != BorderStyle.None)
        {
            if (this.IsMasked) this.PhosphorGraphics.BeginMask(this.MaskingRect, this.MaskingMode);

            Rect worldRect = this.rectTransform.GetWorldSpaceRect();

            Rect rect = this.canvas.rootCanvas.CanvasToOscilloscopeSpace(worldRect);

            if (this.BorderStyle == BorderStyle.Square)
            {
                this.PhosphorGraphics.DrawRect(rect);
            }
            else if (this.BorderStyle == BorderStyle.Rounded)
            {
                this.PhosphorGraphics.DrawRoundRect(rect, this.CornerRadius);
            }

            if (this.IsMasked) this.PhosphorGraphics.EndMask();
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

public enum BorderStyle
{
    None,
    Square,
    Rounded
}
