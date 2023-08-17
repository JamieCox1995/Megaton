using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Stroke")]
public class OscilloscopeStroke : Graphic, IOscilloscopeMaskable {

    public PhosphorGraphics PhosphorGraphics;
    [SerializeField]
    private Vector2[] strokePoints;

    public Vector2[] StrokePoints
    {
        get
        {
            return strokePoints;
        }

        set
        {
            strokePoints = value;
        }
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        this.PhosphorGraphics = FindObjectOfType<PhosphorGraphics>();
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        for (int i = 0; i < this.strokePoints.Length; i++)
        {
            this.strokePoints[i].x = Mathf.Clamp01(this.strokePoints[i].x);
            this.strokePoints[i].y = Mathf.Clamp01(this.strokePoints[i].y);
        }
    }
#endif

    private void Update()
    {
        if (Application.isPlaying)
        {
            Rect worldRect = this.rectTransform.GetWorldSpaceRect();

            Rect rect = this.canvas.rootCanvas.CanvasToOscilloscopeSpace(worldRect);

            Vector2[] points = ConvertRelativePoints(this.StrokePoints, rect);

            if (this.IsMasked) this.PhosphorGraphics.BeginMask(this.MaskingRect, this.MaskingMode);
            this.PhosphorGraphics.DrawPolyLine(points);
            if (this.IsMasked) this.PhosphorGraphics.EndMask();
        }
    }

    private Vector2[] ConvertRelativePoints(Vector2[] points, Rect rect)
    {
        return points.Select(p => Rect.NormalizedToPoint(rect, p)).ToArray();
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
