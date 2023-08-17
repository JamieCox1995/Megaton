//using GeoJSON;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.UI;

//public class WorldMap : Graphic, IOscilloscopeMaskable
//{
//    public PhosphorGraphics PhosphorGraphics;
//    public TextAsset geoJson;

//    FeatureCollection featureCollection;

//#if UNITY_EDITOR
//    protected override void Reset()
//    {
//        base.Reset();

//        this.PhosphorGraphics = FindObjectOfType<PhosphorGraphics>();
//    }
//#endif

//    // Use this for initialization
//    protected override void Start()
//    {
//        base.Start();
//        featureCollection = GeoJSONObject.Deserialize(geoJson.text);
//	}
	
//	// Update is called once per frame
//	void Update()
//    {
//        if (Application.isPlaying)
//        {
//            foreach (var feature in featureCollection.features)
//            {
//                switch (feature.geometry.type)
//                {
//                    case "MultiPolygon":
//                    case "Polygon":
//                    case "LineString":
//                    case "MultiLineString":

//                        var multiPolygon = feature.geometry as ArrayArrayGeometryObject;

//                        foreach (var poly in multiPolygon.coordinates)
//                        {
//                            Vector2[] points = poly.Select(pt => new Vector2(pt.longitude / 180f, pt.latitude / 180f)).ToArray();

//                            if (points.Length >= 2) this.PhosphorGraphics.DrawPolyLine(points);
//                        }

//                        break;

//                    default:
//                        break;
//                }
//            }
//        }
//	}

//    private void ClearGraphics()
//    {
//        Debug.LogFormat(this, "Graphics cleared by {0}.", this.name);
//        this.PhosphorGraphics.ClearScreen();
//    }

//    protected override void OnRectTransformDimensionsChange()
//    {
//        base.OnRectTransformDimensionsChange();

//        ClearGraphics();
//    }

//    #region IOscilloscopeMaskable implementation
//    private Rect? maskingRect;

//    public MaskingMode MaskingMode { get; private set; }
//    public Rect MaskingRect { get { return maskingRect ?? Rect.zero; } }
//    public bool IsMasked { get { return maskingRect != null; } }

//    public void SetMaskingRect(Rect rect)
//    {
//        maskingRect = rect;
//    }

//    public void SetMaskingMode(MaskingMode mode)
//    {
//        this.MaskingMode = mode;
//    }

//    public void ClearMaskingRect()
//    {
//        maskingRect = null;
//    }
//    #endregion
//}
