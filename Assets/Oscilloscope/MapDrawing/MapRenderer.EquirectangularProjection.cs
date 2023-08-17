using GeoJSON.Net.Geometry;
using UnityEngine;

public partial struct MapRenderer
{
    private class EquirectangularProjection : IProjection
    {
        private float scale = 1f;

        private float ScaleMultiplier { get { return scale / 180f; } }

        public EquirectangularProjection(float scale)
        {
            this.scale = scale;
        }

        public Vector2 Project(IPosition position)
        {
            return new Vector2((float)position.Longitude * this.ScaleMultiplier, (float)position.Latitude * this.ScaleMultiplier);
        }

        public bool IsPositionVisible(IPosition position)
        {
            return true;
        }

        public Vector2[] GetProjectionOutline()
        {
            return new Vector2[]
            {
                new Vector2(-scale, -scale / 2f),
                new Vector2(scale, -scale / 2f),
                new Vector2(scale, scale / 2f),
                new Vector2(-scale, scale / 2f),
                new Vector2(-scale, -scale / 2f),
            };
        }

        public Vector2[] GetGraticule(float angle, GraticuleType type)
        {
            switch (type)
            {
                case GraticuleType.Meridian:
                    return new Vector2[]
                    {
                        new Vector2(angle * this.ScaleMultiplier, scale / 2f),
                        new Vector2(angle * this.ScaleMultiplier, -scale / 2f),
                    };

                case GraticuleType.Parallel:
                default:
                    return new Vector2[]
                    {
                        new Vector2(-scale, angle * this.ScaleMultiplier),
                        new Vector2(scale, angle * this.ScaleMultiplier),
                    };
            }
        }
    }
}
