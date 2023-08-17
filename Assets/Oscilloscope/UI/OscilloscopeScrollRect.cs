using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Scroll Rect")]
public class OscilloscopeScrollRect : ScrollRect, IOscilloscopeMaskable
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
            Rect worldRect = this.rectTransform.GetWorldSpaceRect();

            if (this.IsMasked) this.PhosphorGraphics.BeginMask(this.MaskingRect, this.MaskingMode);
            this.PhosphorGraphics.DrawRect(this.canvas.CanvasToOscilloscopeSpace(worldRect));
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
