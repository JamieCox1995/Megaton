using System.Collections.Generic;
using UnityEngine;

public partial class Typesetter
{
    private interface IBlockTextElement : ITextElement
    {
        IEnumerable<Rect> GetAllRects(Rect layoutRect, ref Vector2 cursorPosition);
    }
}