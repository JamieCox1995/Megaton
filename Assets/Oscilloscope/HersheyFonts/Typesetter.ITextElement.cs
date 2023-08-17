using System;
using UnityEngine;

public partial class Typesetter
{
    private interface ITextElement
    {
        TextAnchor TextAnchor { get; }
        Vector2 PreferredSize { get; }
        float PreferredWidth { get; }
        float PreferredHeight { get; }

        Rect GetRect(Rect layoutRect, ref Vector2 cursorPosition);
        void Render(Action<Vector2[]> strokeDrawingFunc, Rect layoutRect);
    }
}