using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Typesetter
{
    private class BlockElement : BlockTextElement<ParagraphElement>
    {
        public override Vector2 PreferredSize
        {
            get
            {
                Vector2 size = Vector2.zero;

                foreach (var paragraph in this.Elements)
                {
                    size.x = Mathf.Max(size.x, paragraph.PreferredWidth);
                    size.y += paragraph.PreferredHeight;
                }

                return size;
            }
        }

        public override float PreferredWidth { get { return this.PreferredSize.x; } }
        public override float PreferredHeight { get { return this.PreferredSize.y; } }

        public BlockElement(TextAnchor anchor)
        {
            this.TextAnchor = anchor;
        }

        public void Populate(string str, HersheyFont font, float fontSize, float lineSpacing)
        {
            this.Elements.Clear();
            this.CachedElementRects.Clear();

            if (string.IsNullOrEmpty(str)) return;

            string[] paragraphStrings = str.Split(new[] { '\n', '\r' }, StringSplitOptions.None);
            foreach (var paragraphString in paragraphStrings)
            {
                string internalStr = paragraphString;

                if (string.IsNullOrEmpty(internalStr)) internalStr = " ";

                var paragraph = new ParagraphElement(this.TextAnchor);
                paragraph.Populate(internalStr, font, fontSize, lineSpacing);
                this.Elements.Add(paragraph);
            }
        }

        public override IEnumerable<Rect> GetAllRects(Rect layoutRect, ref Vector2 cursorPosition)
        {
            if (this.CachedElementRects.Count == 0)
            {
                if (layoutRect.size == Vector2.zero) return Enumerable.Empty<Rect>();

                List<Rect> allRects = new List<Rect>();

                foreach (var paragraph in this.Elements)
                {
                    allRects.AddRange(paragraph.GetAllRects(layoutRect, ref cursorPosition));
                }

                this.CachedElementRects.AddRange(AlignParagraphs(allRects, layoutRect));
            }

            return this.CachedElementRects;
        }

        public override void Render(Action<Vector2[]> strokeDrawingFunc, Rect layoutRect)
        {
            Vector2 cursorPosition = GetRectAnchor(layoutRect, this.TextAnchor);

            var paragraphRects = new List<Rect>();
            foreach (var paragraph in this.Elements)
            {
                paragraphRects.Add(paragraph.GetRect(layoutRect, ref cursorPosition));
            }

            var alignedParagraphRects = AlignParagraphs(paragraphRects, layoutRect).ToArray();

            for (int i = 0; i < this.Elements.Count; i++)
            {
                this.Elements[i].Render(strokeDrawingFunc, alignedParagraphRects[i]);
            }
        }

        private IEnumerable<Rect> AlignParagraphs(IEnumerable<Rect> paragraphRects, Rect layoutRect)
        {
            // Line is already top-aligned by default.
            if (IsTopAligned(this.TextAnchor)) return paragraphRects;
            if (!paragraphRects.Any()) return paragraphRects;

            var rects = paragraphRects.ToArray();

            float blockYMin = rects.Min(r => r.yMin);
            float blockYHeight = rects.Max(r => r.yMax);

            float blockHeight = blockYHeight - blockYMin;

            // The offset for bottom-aligned text.
            float delta = layoutRect.height - blockHeight;

            // Distribute the delta for middle-aligned text.
            if (IsMiddleAligned(this.TextAnchor)) delta *= 0.5f;

            return rects.Select(r =>
            {
                r.yMin -= delta;
                r.yMax -= delta;
                return r;
            });
        }

        public override string ToString()
        {
            return this.Elements.AllToString();
        }
    }
}