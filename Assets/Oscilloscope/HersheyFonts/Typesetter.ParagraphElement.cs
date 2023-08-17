using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Typesetter
{
    private class ParagraphElement : BlockTextElement<IInlineTextElement>
    {
        public override Vector2 PreferredSize
        {
            get
            {
                Vector2 size = Vector2.zero;
                
                foreach (var inlineElement in Elements)
                {
                    size.x += inlineElement.PreferredWidth;
                    size.y = Mathf.Max(size.y, inlineElement.PreferredHeight);
                }

                return size;
            }
        }

        public override float PreferredWidth { get { return this.PreferredSize.x; } }
        public override float PreferredHeight { get { return this.PreferredSize.y; } }

        public ParagraphElement(TextAnchor anchor)
        {
            this.TextAnchor = anchor;
        }

        public void Populate(string str, HersheyFont font, float fontSize, float lineSpacing)
        {
            foreach (var textElementStr in GetTextElements(str))
            {
                if (IsWhitespace(textElementStr))
                {
                    this.Elements.Add(new SpaceElement(font, fontSize, lineSpacing, this.TextAnchor));
                }
                else
                {
                    this.Elements.Add(new CharacterElement(textElementStr, font, fontSize, lineSpacing, this.TextAnchor));
                }
            }
        }

        public override IEnumerable<Rect> GetAllRects(Rect layoutRect, ref Vector2 cursorPosition)
        {
            if (layoutRect.size == Vector2.zero) return Enumerable.Empty<Rect>();

            if (this.CachedElementRects.Count == 0)
            {

                List<int> lineBreakIndices = new List<int>();

                int lastSpaceIndex = -1;
                for (int i = 0; i < this.Elements.Count; i++)
                {
                    var element = Elements[i];

                    var rect = element.GetRect(layoutRect, ref cursorPosition);

                    var lastLineBreak = lineBreakIndices.LastOrDefault();

                    bool shouldAddElement = true;
                    if (ShouldBreakLine(layoutRect, cursorPosition) && (i - lastLineBreak) > 0)
                    {
                        int breakIndex = lastSpaceIndex < 0 ? i : lastSpaceIndex;

                        int count = i - breakIndex - 1;

                        if (count > 0)
                        {
                            shouldAddElement = false;

                            // Remove all rects that are after the break to reposition them.
                            for (int j = 0; j < count; j++)
                            {
                                this.CachedElementRects.RemoveAt(breakIndex);
                            }

                            float lineHeight = GetMaxInlineElementHeight(i, lastLineBreak);

                            UpdateCursorPosition(lineHeight, layoutRect, ref cursorPosition);

                            lineBreakIndices.Add(breakIndex);

                            i = breakIndex;

                            lastSpaceIndex = -1;
                        }
                    }

                    if (shouldAddElement)
                    {
                        if (element is SpaceElement) lastSpaceIndex = i;

                        this.CachedElementRects.Add(rect);
                    }
                }

                Debug.AssertFormat(this.CachedElementRects.Count == this.Elements.Count, "The number of Rect objects ({0}) does not match the number of inline elements ({1}).", this.CachedElementRects.Count, this.Elements.Count);

                lineBreakIndices.Add(this.CachedElementRects.Count);

                // Align all the lines.
                int previousIndex = 0;
                foreach (var index in lineBreakIndices)
                {
                    int lineElementCount = index - previousIndex;

                    AlignLine(layoutRect, this.CachedElementRects, previousIndex, lineElementCount);

                    previousIndex = index + 1;
                }

                lineBreakIndices.RemoveAt(lineBreakIndices.Count - 1);

                // Force a line break at the end of the paragraph.
                float lastLineHeight = GetMaxInlineElementHeight(this.Elements.Count - 1, lineBreakIndices.LastOrDefault());
                UpdateCursorPosition(lastLineHeight, layoutRect, ref cursorPosition);
            }

            return this.CachedElementRects;
        }

        public override void Render(Action<Vector2[]> strokeDrawingFunc, Rect layoutRect)
        {
            Vector2 cursorPosition = GetRectAnchor(layoutRect, this.TextAnchor);

            var rects = GetAllRects(layoutRect, ref cursorPosition).ToArray();

            int count = Mathf.Min(rects.Length, this.Elements.Count);

            for (int i = 0; i < count; i++)
            {
                Elements[i].Render(strokeDrawingFunc, rects[i]);
            }
        }

        private float GetMaxInlineElementHeight(int currentBreakIndex, int previousBreakIndex)
        {
            float lineHeight = 0f;
            for (int j = previousBreakIndex; j <= currentBreakIndex; j++)
            {
                float currentElementPreferredHeight = Elements[j].PreferredHeight;
                if (currentElementPreferredHeight > lineHeight) lineHeight = currentElementPreferredHeight;
            }

            return lineHeight;
        }

        private bool ShouldBreakLine(Rect layoutRect, Vector2 cursorPosition)
        {
            // Apply a tolerance to account for floating point weirdness.
            float tolerance = 0.025f;
            return cursorPosition.x > layoutRect.xMax + tolerance;
        }

        private void UpdateCursorPosition(float lineHeight, Rect layoutRect, ref Vector2 cursorPosition)
        {
            cursorPosition.x = layoutRect.xMin;
            cursorPosition.y -= lineHeight;
        }

        private void AlignLine(Rect layoutRect, List<Rect> inlineElementRects, int lineBreakIndex, int count)
        {
            var lineRects = this.CachedElementRects.GetRange(lineBreakIndex, count);

            var alignedRects = AlignRects(lineRects, layoutRect).ToArray();

            for (int j = 0; j < count; j++)
            {
                inlineElementRects[j + lineBreakIndex] = alignedRects[j];
            }
        }

        private IEnumerable<Rect> AlignRects(IEnumerable<Rect> lineRects, Rect layoutRect)
        {
            // Line is already left-aligned by default.
            if (IsLeftAligned(this.TextAnchor)) return lineRects;
            if (!lineRects.Any()) return lineRects;

            var rects = lineRects.ToArray();

            float lineXMin = rects.Min(r => r.xMin);
            float lineXMax = rects.Max(r => r.xMax);

            float lineWidth = lineXMax - lineXMin;

            // The offset for right-aligned text.
            float delta = layoutRect.width - lineWidth;

            // Distribute the delta for center-aligned text.
            if (IsCenterAligned(this.TextAnchor)) delta *= 0.5f;

            return rects.Select(r =>
            {
                r.xMin += delta;
                r.xMax += delta;
                return r;
            });
        }

        public override string ToString()
        {
            return this.Elements.AllToString();
        }
    }
}