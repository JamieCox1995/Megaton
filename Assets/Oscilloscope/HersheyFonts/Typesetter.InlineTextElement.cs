using System;
using UnityEngine;

public partial class Typesetter
{
    private abstract class InlineTextElement : TextElement, IInlineTextElement
    {
        public override Vector2 PreferredSize { get { return new Vector2(this.PreferredWidth, this.PreferredHeight); } }
        public override float PreferredHeight { get { return BaseHeight * this.FontSize * this.LineSpacing; } }

        #region IInlineTextElement properties
        public HersheyFont Font { get; private set; }
        public float FontSize { get; private set; }
        public float LineSpacing { get; private set; }
        #endregion

        protected const float BaseHeight = 32f;

        protected InlineTextElement(HersheyFont font, float fontSize, float lineSpacing, TextAnchor anchor)
        {
            this.Font = font;
            this.FontSize = fontSize;
            this.LineSpacing = lineSpacing;
            this.TextAnchor = anchor;
        }

        public override Rect GetRect(Rect layoutRect, ref Vector2 cursorPosition)
        {
            if (!this.HasCachedRect || !this.HasCachedLayoutRect || this.CachedLayoutRect != layoutRect)
            {
                if (layoutRect.size == Vector2.zero) return Rect.zero;

                Rect result = new Rect(cursorPosition.x, cursorPosition.y - this.PreferredHeight, this.PreferredWidth, this.PreferredHeight);

                UpdateCursorPosition(ref cursorPosition);

                this.CachedRect = result;
            }

            return this.CachedRect;
        }

        public override void Render(Action<Vector2[]> strokeDrawingFunc, Rect layoutRect) { }

        private void UpdateCursorPosition(ref Vector2 cursorPosition)
        {
            cursorPosition.x += this.PreferredWidth;
        }

        public abstract override string ToString();
    }
}