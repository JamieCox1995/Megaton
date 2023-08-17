using System;
using UnityEngine;

public partial class Typesetter
{
    private abstract class TextElement : ITextElement
    {
        public virtual TextAnchor TextAnchor { get; protected set; }
        public abstract Vector2 PreferredSize { get; }
        public abstract float PreferredWidth { get; }
        public abstract float PreferredHeight { get; }

        public abstract Rect GetRect(Rect layoutRect, ref Vector2 cursorPosition);
        public abstract void Render(Action<Vector2[]> strokeDrawingFunc, Rect layoutRect);

        private Rect? cachedRect;

        protected bool HasCachedRect { get { return cachedRect != null; } }
        protected Rect CachedRect { get { return cachedRect.Value; } set { cachedRect = value; } }

        private Rect? cachedLayoutRect;

        protected bool HasCachedLayoutRect { get { return cachedLayoutRect != null; } }
        protected Rect CachedLayoutRect { get { return cachedLayoutRect.Value; } set { cachedLayoutRect = value; } }
    }
}