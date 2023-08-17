using GeoJSON.Net.Geometry;
using UnityEngine;

public partial struct MapRenderer
{
    private interface IProjection
    {
        Vector2 Project(IPosition position);
        bool IsPositionVisible(IPosition position);
        Vector2[] GetProjectionOutline();
        Vector2[] GetGraticule(float angle, GraticuleType type);
    }
}
