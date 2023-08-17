using System.Collections.Generic;
using UnityEngine;

namespace TotalDistraction.TextureAtlasCreator
{
    public static class Bake
    {
        public static Result Texture(Settings settings, List<Texture> bakeList)
        {
            Result result = new Result();

            RenderTexture temp = RenderTexture.GetTemporary(settings.Size, settings.Size, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            result.TextureRects = BlitTextures(temp, settings, bakeList);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = temp;

            Texture2D texture = new Texture2D(settings.Size, settings.Size);

            texture.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previous;

            RenderTexture.ReleaseTemporary(temp);

            result.Output = texture;

            return result;
        }

        private static List<Rect> BlitTextures(RenderTexture renderTexture, Settings settings, List<Texture> bakeList)
        {
            Graphics.SetRenderTarget(renderTexture);
            Material mat = new Material(Shader.Find("Unlit/Texture"));

            List<Rect> rects = new List<Rect>();

            for (int i = 0; i < bakeList.Count; i++)
            {
                Texture texture = bakeList[i];

                // Calculate quad coords
                int x = i % settings.XDivisions;
                int y = i / settings.XDivisions;

                float xFrac = (float)x / (float)settings.XDivisions;
                float yFrac = (float)y / (float)settings.YDivisions;

                float nextXFrac = (float)(x + 1) / (float)settings.XDivisions;
                float nextYFrac = (float)(y + 1) / settings.YDivisions;

                Rect r = new Rect { xMin = xFrac, xMax = nextXFrac, yMin = yFrac, yMax = nextYFrac };
                rects.Add(r);

                // Set up renderer
                mat.SetTexture("_MainTex", texture);

                mat.SetPass(0);
                GL.PushMatrix();
                GL.LoadOrtho();

                // Draw quad
                GL.Begin(GL.QUADS);
                GL.TexCoord2(0, 0);
                GL.Vertex3(xFrac, yFrac, 0);
                GL.TexCoord2(0, 1);
                GL.Vertex3(xFrac, nextYFrac, 0);
                GL.TexCoord2(1, 1);
                GL.Vertex3(nextXFrac, nextYFrac, 0);
                GL.TexCoord2(1, 0);
                GL.Vertex3(nextXFrac, yFrac, 0);
                GL.End();

                GL.PopMatrix();
            }

            return rects;
        }

        public class Settings
        {
            public int XDivisions;
            public int YDivisions;

            public int Size;

            public override string ToString()
            {
                return string.Format("{{ XDivisions: {0}, YDivisions: {1}, Size: {3} }}", this.XDivisions, this.YDivisions, this.Size);
            }
        }

        public class Result
        {
            public Texture2D Output;
            public List<Rect> TextureRects;

            public Result()
            {
                this.TextureRects = new List<Rect>();
            }
        }
    }
}