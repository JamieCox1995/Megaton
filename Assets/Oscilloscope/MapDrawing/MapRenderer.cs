using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public partial struct MapRenderer
{
    public float MeridianSpacing;
    public float ParallelSpacing;
    public GeoProjection Projection;
    public float Scale;
    public Vector2 Origin;

    private FeatureCollection featureCollection;

    public void SetGeoJson(TextAsset textAsset)
    {
        featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(textAsset.text);
    }

    public void Render(Action<Vector2[]> strokeRenderFunc)
    {
        IProjection projection = CreateProjection();

        strokeRenderFunc(projection.GetProjectionOutline());

        if (this.MeridianSpacing > 0f)
        {
            float t = this.MeridianSpacing;
            while (t < 180f)
            {
                strokeRenderFunc(projection.GetGraticule(t, GraticuleType.Meridian));
                strokeRenderFunc(projection.GetGraticule(-t, GraticuleType.Meridian));
                t += this.MeridianSpacing;
            }
            strokeRenderFunc(projection.GetGraticule(0, GraticuleType.Meridian));
            strokeRenderFunc(projection.GetGraticule(180f, GraticuleType.Meridian));
        }

        if (this.ParallelSpacing > 0f)
        {
            float t = this.ParallelSpacing;
            while (t < 90f)
            {
                strokeRenderFunc(projection.GetGraticule(t, GraticuleType.Parallel));
                strokeRenderFunc(projection.GetGraticule(-t, GraticuleType.Parallel));
                t += this.ParallelSpacing;
            }
            strokeRenderFunc(projection.GetGraticule(0, GraticuleType.Parallel));
        }

        foreach (var feature in featureCollection.Features)
        {
            if (feature.Geometry == null) continue;

            switch (feature.Geometry.Type)
            {
                case GeoJSONObjectType.MultiPolygon:

                    MultiPolygon multiPolygon = feature.Geometry as MultiPolygon;

                    foreach (var polygon in multiPolygon.Coordinates)
                    {
                        DrawPolygon(polygon, projection, strokeRenderFunc);
                    }

                    break;

                case GeoJSONObjectType.Polygon:

                    DrawPolygon(feature.Geometry as Polygon, projection, strokeRenderFunc);

                    break;

                default:
                    break;
            }
        }
    }

    private void DrawPolygon(Polygon polygon, IProjection projection, Action<Vector2[]> strokeRenderFunc)
    {
        foreach (var lineString in polygon.Coordinates)
        {
            Queue<IPosition> positionQueue = new Queue<IPosition>(lineString.Coordinates);

            List<Vector2> points = new List<Vector2>();
            while (positionQueue.Count > 0)
            {
                IPosition position = positionQueue.Dequeue();

                if (projection.IsPositionVisible(position))
                {
                    points.Add(projection.Project(position));
                }
                else
                {
                    if (points.Count > 0)
                    {
                        strokeRenderFunc(points.ToArray());

                        points.Clear();
                    }
                }
            }

            if (points.Count > 0) strokeRenderFunc(points.ToArray());
        }
    }

    private IProjection CreateProjection()
    {
        switch (this.Projection)
        {
            case GeoProjection.Equirectangular:
            default:
                return new EquirectangularProjection(this.Scale);

            case GeoProjection.AzimuthalOrthographic:
                return new AzimuthalOrthographicProjection(this.Scale, this.Origin.x, this.Origin.y);
        }
    }
}
