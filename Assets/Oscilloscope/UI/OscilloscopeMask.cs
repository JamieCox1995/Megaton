using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Mask")]
public class OscilloscopeMask : UIBehaviour
{
    public PhosphorGraphics PhosphorGraphics;

    [SerializeField, FormerlySerializedAs("MaskingMode")]
    private MaskingMode maskingMode;

    [SerializeField]
    private int maskingParentLevel;

    private RectTransform rectTransform;
    private Canvas canvas;

    public MaskingMode MaskingMode
    {
        get
        {
            return maskingMode;
        }

        set
        {
            maskingMode = value;
            UpdateMasks();
        }
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        this.PhosphorGraphics = FindObjectOfType<PhosphorGraphics>();

        this.rectTransform = GetComponent<RectTransform>();
        this.canvas = GetComponentInParent<Canvas>().rootCanvas;

        UpdateMasks();
    }

    protected override void OnValidate()
    {
        maskingParentLevel = Mathf.Max(0, maskingParentLevel);

        UpdateMasks();
    }
#endif

    protected override void Start()
    {
        base.Start();

        StartCoroutine(StartEnumerator());
    }

    private IEnumerator StartEnumerator()
    {
        yield return null;

        this.rectTransform = GetComponent<RectTransform>();
        this.canvas = GetComponentInParent<Canvas>().rootCanvas;

        UpdateMasks();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (this.rectTransform == null) this.rectTransform = GetComponent<RectTransform>();
        if (this.canvas == null) this.canvas = GetComponentInParent<Canvas>().rootCanvas;

        UpdateMasks();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        UpdateMasks();
    }

    private void UpdateMasks()
    {
        if (this.rectTransform == null) return;

        Rect maskingRect = this.canvas.CanvasToOscilloscopeSpace(this.rectTransform.GetWorldSpaceRect());

        foreach (var maskable in GetMaskingTargets(this.maskingParentLevel))
        {
            maskable.SetMaskingMode(this.MaskingMode);
            maskable.SetMaskingRect(maskingRect);
        }
    }

    private IEnumerable<IOscilloscopeMaskable> GetMaskingTargets(int parentLevel)
    {
        return transform
            .GetAncestor(parentLevel)
            .GetSearchHierarchy<OscilloscopeMask>()
            .GetAllComponents<IOscilloscopeMaskable>();

    }

    private void ClearMasks()
    {
        foreach (var maskable in GetMaskingTargets(this.maskingParentLevel))
        {
            maskable.SetMaskingMode(default(MaskingMode));
            maskable.ClearMaskingRect();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ClearMasks();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        ClearMasks();
    }
}

public interface IOscilloscopeMaskable
{
    bool IsMasked { get; }
    MaskingMode MaskingMode { get; }
    Rect MaskingRect { get; }
    void SetMaskingMode(MaskingMode mode);
    void SetMaskingRect(Rect rect);
    void ClearMaskingRect();
}

public enum MaskingMode
{
    MaskOutside,
    MaskInside
}