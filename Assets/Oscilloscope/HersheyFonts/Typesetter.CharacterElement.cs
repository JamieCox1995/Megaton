using System;
using System.Linq;
using UnityEngine;

public partial class Typesetter
{
    private class CharacterElement : InlineTextElement
    {
        private string characterString;
        private HersheyCharacter glyph;

        public override float PreferredWidth { get { return glyph == null ? 0f : (glyph.Width * this.FontSize); } }

        public CharacterElement(string character, HersheyFont font, float fontSize, float lineSpacing, TextAnchor anchor) : base(font, fontSize, lineSpacing, anchor)
        {
            if (character == null) throw new ArgumentNullException("character");

            if (IsWhitespace(character)) throw new ArgumentException("Use the SpaceElement class instead for spaces.", "character");

            glyph = font.GetCharacterFromString(character);
            characterString = character;
        }

        public override void Render(Action<Vector2[]> strokeDrawingFunc, Rect layoutRect)
        {
            if (glyph == null) return;

            if (layoutRect.size == Vector2.zero) return;

            foreach (var stroke in glyph.Strokes)
            {
                Vector2[] points = stroke.Points.Select(p =>
                {
                    float tx = Mathf.InverseLerp(glyph.Bounds.x, -glyph.Bounds.y, p.x);
                    float ty = Mathf.InverseLerp(-16f, 16f, p.y);

                    float yMax = layoutRect.yMax;
                    float yMin = layoutRect.yMax - BaseHeight * this.FontSize;

                    float x = Mathf.Lerp(layoutRect.xMin, layoutRect.xMax, tx);
                    float y = Mathf.Lerp(yMin, layoutRect.yMax, ty);

                    return new Vector2(x, y);
                }).ToArray();

                strokeDrawingFunc(points);
            }

            base.Render(strokeDrawingFunc, layoutRect);
        }

        public override string ToString()
        {
            return characterString;
        }
    }
}