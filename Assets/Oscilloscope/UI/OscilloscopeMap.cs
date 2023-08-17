using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Map")]
public class OscilloscopeMap : Graphic, IOscilloscopeMaskable
{
    public PhosphorGraphics PhosphorGraphics;
    public TextAsset GeoJsonAsset;
    public MapRenderer MapRenderer;

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

        this.MapRenderer.SetGeoJson(this.GeoJsonAsset);
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            Rect worldRect = this.rectTransform.GetWorldSpaceRect();

            Rect rect = this.canvas.rootCanvas.CanvasToOscilloscopeSpace(worldRect);

            Action<Vector2[]> drawFunc = pts =>
            {
                if (pts.Length < 2) return;

                this.PhosphorGraphics.DrawPolyLine(ConvertRelativePoints(pts, rect));
            };

            if (this.IsMasked) this.PhosphorGraphics.BeginMask(this.MaskingRect, this.MaskingMode);
            this.MapRenderer.Render(drawFunc);
            if (this.IsMasked) this.PhosphorGraphics.EndMask();
        }
    }

    private Vector2[] ConvertRelativePoints(Vector2[] points, Rect rect)
    {
        return points.Select(p => Rect.NormalizedToPoint(rect, (p + Vector2.one) * 0.5f)).ToArray();
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
