using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Spinner")]
public class OscilloscopeSpinner : Graphic, IOscilloscopeMaskable
{

    public PhosphorGraphics PhosphorGraphics;

    [Range(0f, 1f)]
    public float InnerRadius = 0.25f;
    [Range(0f, 1f)]
    public float OuterRadius = 1f;
    public float Speed1;
    public float Speed2;
    public TurningDirection Direction;
    public int Resolution = 32;


    private float speedStart;
    private float speedEnd;
    private float startAngle;
    private float endAngle;

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        this.PhosphorGraphics = FindObjectOfType<PhosphorGraphics>();
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (this.InnerRadius > this.OuterRadius)
        {
            float temp = this.InnerRadius;
            this.InnerRadius = this.OuterRadius;
            this.OuterRadius = temp;
        }

        this.Speed1 = Mathf.Max(this.Speed1, 0f);
        this.Speed2 = Mathf.Max(this.Speed2, 0f);

        this.Resolution = Mathf.Max(this.Resolution, 4);
    }
#endif

    private void Start()
    {
        speedStart = Mathf.Min(this.Speed1, this.Speed2);
        speedEnd = Mathf.Max(this.Speed1, this.Speed2);
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (Mathf.Abs(startAngle - endAngle) >= 360f)
            {
                float temp = speedStart;
                speedStart = speedEnd;
                speedEnd = temp;

                startAngle %= 360f;
                endAngle %= 360f;
            }

            startAngle += speedStart * Time.deltaTime;
            endAngle += speedEnd * Time.deltaTime;

            Rect worldRect = this.rectTransform.GetWorldSpaceRect();

            Rect rect = this.canvas.rootCanvas.CanvasToOscilloscopeSpace(worldRect);

            Vector2[] points = ConvertRelativePoints(GetStroke(), rect);

            if (this.IsMasked) this.PhosphorGraphics.BeginMask(this.MaskingRect, this.MaskingMode);
            this.PhosphorGraphics.DrawPolyLine(points);
            if (this.IsMasked) this.PhosphorGraphics.EndMask();
        }
    }

    private Vector2[] ConvertRelativePoints(Vector2[] points, Rect rect)
    {
        return points.Select(p => Rect.NormalizedToPoint(rect, p)).ToArray();
    }

    private Vector2[] GetStroke()
    {
        float start = startAngle;
        float end = endAngle;

        if (start > end) end += 360f;

        int arcPointCount = Mathf.CeilToInt(((end - start) * this.Resolution) / 360f);

        start *= Mathf.Deg2Rad;
        end *= Mathf.Deg2Rad;

        Vector2[] result = new Vector2[(arcPointCount + 1) * 2 + 1];

        float turningFactor = (this.Direction == TurningDirection.Counterclockwise) ? 1 : -1;

        for (int i = 0; i <= arcPointCount; i++)
        {
            float t = Mathf.Lerp(start, end, (float)i / (float)arcPointCount) * turningFactor;

            float innerX = 0.5f + 0.5f * this.InnerRadius * Mathf.Cos(t);
            float innerY = 0.5f + 0.5f * this.InnerRadius * Mathf.Sin(t);

            result[i] = new Vector2(innerX, innerY);
        }

        for (int i = 0; i <= arcPointCount; i++)
        {
            float t = Mathf.Lerp(end, start, (float)i / (float)arcPointCount) * turningFactor;

            float outerX = 0.5f + 0.5f * this.OuterRadius * Mathf.Cos(t);
            float outerY = 0.5f + 0.5f * this.OuterRadius * Mathf.Sin(t);

            result[arcPointCount + i + 1] = new Vector2(outerX, outerY);
        }

        result[(arcPointCount + 1) * 2] = result[0];

        return result;
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

    public enum TurningDirection
    {
        Clockwise,
        Counterclockwise,
    }
}
