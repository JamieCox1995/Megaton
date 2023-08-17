using GeoJSON.Net.Geometry;
using System.Collections.Generic;
using UnityEngine;

public partial struct MapRenderer
{
    private class AzimuthalOrthographicProjection : IProjection
    {
        private float r = 1f;
        private float lambda0 = 0f;
        private float phi0 = 0f;

        public AzimuthalOrthographicProjection(float radius, IPosition origin)
        {
            r = radius;
            lambda0 = Mathf.Deg2Rad * (float)origin.Longitude;
            phi0 = Mathf.Deg2Rad * (float)origin.Latitude;
        }

        public AzimuthalOrthographicProjection(float radius, float longitude, float latitude)
        {
            r = radius;
            lambda0 = Mathf.Deg2Rad * longitude;
            phi0 = Mathf.Deg2Rad * latitude;
        }

        public Vector2 Project(IPosition position)
        {
            float lambda = Mathf.Deg2Rad * (float)position.Longitude;
            float phi = Mathf.Deg2Rad * (float)position.Latitude;

            return ProjectInternal(lambda, phi);
        }

        private Vector2 ProjectInternal(float lambda, float phi)
        {
            float x = r * Mathf.Cos(phi) * Mathf.Sin(lambda - lambda0);
            float y = r * (Mathf.Cos(phi0) * Mathf.Sin(phi) - Mathf.Sin(phi0) * Mathf.Cos(phi) * Mathf.Cos(lambda - lambda0));

            return new Vector2(x, y);
        }

        public bool IsPositionVisible(IPosition position)
        {
            float lambda = Mathf.Deg2Rad * (float)position.Longitude;
            float phi = Mathf.Deg2Rad * (float)position.Latitude;
            return IsPositionVisibleInternal(lambda, phi);
        }

        private bool IsPositionVisibleInternal(float lambda, float phi)
        {
            return (Mathf.Sin(phi0) * Mathf.Sin(phi) + Mathf.Cos(phi0) * Mathf.Cos(phi) * Mathf.Cos(lambda - lambda0)) >= 0;
        }

        public Vector2[] GetProjectionOutline()
        {
            const int outlinePoints = 64;

            List<Vector2> result = new List<Vector2>(outlinePoints + 1);
            for (int i = 0; i <= outlinePoints; i++)
            {
                float t = 2f * Mathf.PI * ((float)i / (float)outlinePoints);

                result.Add(new Vector2(Mathf.Sin(t) * r, Mathf.Cos(t) * r));
            }

            return result.ToArray();
        }

        public Vector2[] GetGraticule(float angle, GraticuleType type)
        {
            const int outlinePoints = 64;

            float alpha = Mathf.Deg2Rad * angle;

            List<Vector2> result = new List<Vector2>();
            switch (type)
            {
                case GraticuleType.Meridian:
                    for (int i = 0; i <= outlinePoints / 2; i++)
                    {
                        float t = 2f * Mathf.PI * ((float)i / (float)outlinePoints) - (Mathf.PI * 0.5f);

                        float lambda = alpha;
                        float phi = t + phi0;

                        if (IsPositionVisibleInternal(lambda, phi)) result.Add(ProjectInternal(lambda, phi));
                    }
                    break;

                case GraticuleType.Parallel:
                default:
                    for (int i = 0; i <= outlinePoints; i++)
                    {
                        float t = 2f * Mathf.PI * ((float)i / (float)outlinePoints);

                        float lambda = t + GetAntipodeLongitude(lambda0);
                        float phi = alpha;

                        if (IsPositionVisibleInternal(lambda, phi)) result.Add(ProjectInternal(lambda, phi));
                    }
                    break;
            }

            return result.ToArray();
        }

        private float GetAntipodeLongitude(float lambda)
        {
            return (Mathf.PI - Mathf.Abs(lambda)) * -Mathf.Sign(lambda);
        }
    }
}
