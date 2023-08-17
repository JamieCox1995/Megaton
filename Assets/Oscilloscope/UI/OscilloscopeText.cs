using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Text")]
public class OscilloscopeText : Graphic, ILayoutElement, IOscilloscopeMaskable
{
    public PhosphorGraphics PhosphorGraphics;
    public Typesetter Typesetter;
    [TextArea(3, 10)]
    public string Text;

    private Rect? maskingRect;

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        this.PhosphorGraphics = FindObjectOfType<PhosphorGraphics>();
        this.Typesetter.Font = Resources.Load<HersheyFont>("Fonts/futural");
        //this.Typesetter.FontSize = 14;
        //this.Typesetter.LineSpacing = 1;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (Application.isPlaying) ClearGraphics();
    }
#endif

    protected override void Start()
    {
        base.Start();

        this.Typesetter.SettingsChanged += ClearGraphics;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            if (string.IsNullOrEmpty(this.Text)) return;
            if (this.rectTransform == null) return;

            Rect worldRect = this.rectTransform.GetWorldSpaceRect();

            Rect rect = this.canvas.rootCanvas.CanvasToOscilloscopeSpace(worldRect);

            if (this.IsMasked) this.PhosphorGraphics.BeginMask(this.MaskingRect, this.MaskingMode);
            this.Typesetter.RenderText(this.Text, rect, this.PhosphorGraphics.DrawPolyLine);
            if (this.IsMasked) this.PhosphorGraphics.EndMask();
        }
    }

    private void ClearGraphics()
    {
        this.Typesetter.ClearCache();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        ClearGraphics();
    }

    public float minWidth { get { return 0; } }

    public float preferredWidth { get { return this.Typesetter.GetPreferredRect(this.Text, this.rectTransform.position).width; } }

    public float flexibleWidth { get { return -1; } }

    public float minHeight { get { return 0; } }

    public float preferredHeight { get { return this.Typesetter.GetPreferredRect(this.Text, this.rectTransform.position).height; } }

    public float flexibleHeight { get { return -1; } }

    public int layoutPriority { get { return 0; } }

    public virtual void CalculateLayoutInputHorizontal() { }

    public virtual void CalculateLayoutInputVertical() { }

    #region IOscilloscopeMaskable implementation
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
