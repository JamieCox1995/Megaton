using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalDistraction.TextureAtlasCreator
{
    public static class UVRemap
    {
        public static Mesh Forward(Mesh original, Rect uvRect)
        {
            Mesh result = Mesh.Instantiate(original);
            result.name = original.name;

            Vector2[] originalUvs = original.uv;

            result.uv = originalUvs.Select(uv => Lerp2D(uvRect.min, uvRect.max, uv)).ToArray();

            return result;
        }

        public static Mesh Forward(Mesh original, Rect[] uvRects)
        {
            if (original.subMeshCount != uvRects.Length) throw new System.ArgumentException();

            Mesh result = Mesh.Instantiate(original);
            result.name = original.name;

            Vector2[] originalUvs = original.uv;
            Dictionary<int, Vector2> uvs = new Dictionary<int, Vector2>();

            for (int i = 0; i < original.subMeshCount; i++)
            {
                HashSet<int> triangles = new HashSet<int>(original.GetTriangles(i));
                Rect uvRect = uvRects[i];

                foreach (int index in triangles)
                {
                    Vector2 uv = originalUvs[index];

                    uvs.Add(index, Lerp2D(uvRect.min, uvRect.max, uv));
                }
            }

            result.uv = uvs.OrderBy(uv => uv.Key).Select(uv => uv.Value).ToArray();

            return result;
        }

        public static Mesh Reverse(Mesh original, Rect uvRect)
        {
            Mesh result = Mesh.Instantiate(original);
            result.name = original.name;

            Vector2[] originalUVs = original.uv;

            result.uv = originalUVs.Select(uv => InverseLerp2D(uvRect.min, uvRect.max, uv)).ToArray();

            return result;
        }

        private static Vector2 Lerp2D(Vector2 a, Vector2 b, Vector2 t)
        {
            float x = Mathf.Lerp(a.x, b.x, t.x);
            float y = Mathf.Lerp(a.y, b.y, t.y);

            return new Vector2(x, y);
        }

        private static Vector2 InverseLerp2D(Vector2 a, Vector2 b, Vector2 value)
        {
            float tX = Mathf.InverseLerp(a.x, b.x, value.x);
            float tY = Mathf.InverseLerp(a.y, b.y, value.y);

            return new Vector2(tX, tY);
        }
    }
}