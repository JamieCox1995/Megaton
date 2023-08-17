using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Typesetter
{
    private abstract class BlockTextElement<TElement> : TextElement, IBlockTextElement
        where TElement : ITextElement
    {
        protected List<TElement> Elements { get; private set; }
        protected List<Rect> CachedElementRects { get; private set; }

        protected BlockTextElement()
        {
            this.Elements = new List<TElement>();
            this.CachedElementRects = new List<Rect>();
        }

        public override Rect GetRect(Rect layoutRect, ref Vector2 cursorPosition)
        {
            if (!this.HasCachedRect || !this.HasCachedLayoutRect || this.CachedLayoutRect != layoutRect)
            {
                var elementRects = GetAllRects(layoutRect, ref cursorPosition).ToArray();

                if (elementRects.Length == 0) return Rect.zero;

                float xmin = elementRects.Min(r => r.xMin);
                float xmax = elementRects.Max(r => r.xMax);
                float ymin = elementRects.Min(r => r.yMin);
                float ymax = elementRects.Max(r => r.yMax);

                this.CachedRect = Rect.MinMaxRect(xmin, ymin, xmax, ymax);
            }

            return this.CachedRect;
        }

        public abstract IEnumerable<Rect> GetAllRects(Rect layoutRect, ref Vector2 cursorPosition);
    }
}