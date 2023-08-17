using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

[Serializable]
public partial class Typesetter
{
    [UnityEngine.Serialization.FormerlySerializedAs("Font")]
    [SerializeField]
    private HersheyFont font;
    [UnityEngine.Serialization.FormerlySerializedAs("FontSize")]
    [SerializeField]
    private float fontSize = 14f;
    [UnityEngine.Serialization.FormerlySerializedAs("Tracking")]
    [SerializeField]
    private float tracking = 0f;
    [UnityEngine.Serialization.FormerlySerializedAs("LineSpacing")]
    [SerializeField]
    private float lineSpacing = 1f;
    [UnityEngine.Serialization.FormerlySerializedAs("TextAnchor")]
    [SerializeField]
    private TextAnchor textAnchor = TextAnchor.MiddleCenter;
    [UnityEngine.Serialization.FormerlySerializedAs("VerticalWrapMode")]
    [SerializeField]
    private VerticalWrapMode verticalWrapMode = VerticalWrapMode.Truncate;
    [UnityEngine.Serialization.FormerlySerializedAs("PixelsPerUnit")]
    [SerializeField]
    private float pixelsPerUnit = 100;

    private const float CharacterBaseHeight = 32;

    private float FontScale { get { return FontSize / (CharacterBaseHeight * PixelsPerUnit); } }
    private float LineHeight { get { return FontSize * LineSpacing / PixelsPerUnit; } }

    public event Action SettingsChanged;

    private string cachedString;
    private BlockElement cachedBlock;
    private Rect? cachedRect;

    private bool HasCachedRect { get { return cachedRect != null; } }
    private Rect CachedRect { get { return cachedRect.Value; } set { cachedRect = value; } }

    public HersheyFont Font
    {
        get
        {
            return font;
        }

        set
        {
            if (font != value) OnSettingsChanged();
            font = value;
        }
    }

    public float FontSize
    {
        get
        {
            return fontSize;
        }

        set
        {
            if (fontSize != value) OnSettingsChanged();
            fontSize = value;
        }
    }

    public float Tracking
    {
        get
        {
            return tracking;
        }

        set
        {
            if (tracking != value) OnSettingsChanged();
            tracking = value;
        }
    }

    public float LineSpacing
    {
        get
        {
            return lineSpacing;
        }

        set
        {
            if (lineSpacing != value) OnSettingsChanged();
            lineSpacing = value;
        }
    }

    public TextAnchor TextAnchor
    {
        get
        {
            return textAnchor;
        }

        set
        {
            if (textAnchor != value) OnSettingsChanged();
            textAnchor = value;
        }
    }

    public VerticalWrapMode VerticalWrapMode
    {
        get
        {
            return verticalWrapMode;
        }

        set
        {
            if (verticalWrapMode != value) OnSettingsChanged();
            verticalWrapMode = value;
        }
    }

    public float PixelsPerUnit
    {
        get
        {
            return pixelsPerUnit;
        }

        set
        {
            if (pixelsPerUnit != value) OnSettingsChanged();
            pixelsPerUnit = value;
        }
    }

    private const float PreferredRectMaxSize = 1024f * 1024f;

    public Rect GetPreferredRect(string str, Vector2 position)
    {
        if (this.Font == null) throw new InvalidOperationException("The typesetter has no font.");

        if (string.IsNullOrEmpty(str)) return new Rect(position, Vector2.zero);

        ClearCache();

        CreateCachedBlock(str);

        // Align the block horizontally
        float xmin, xmax;
        if (IsLeftAligned(this.TextAnchor))
        {
            xmin = position.x;
            xmax = position.x + cachedBlock.PreferredWidth;
        }
        else if (IsCenterAligned(this.TextAnchor))
        {
            xmin = position.x - cachedBlock.PreferredWidth * 0.5f;
            xmax = position.x + cachedBlock.PreferredWidth * 0.5f;
        }
        else if (IsRightAligned(this.TextAnchor))
        {
            xmin = position.x - cachedBlock.PreferredWidth;
            xmax = position.x;
        }
        else
        {
            throw new InvalidOperationException("Invalid text anchor specified to the typesetter.");
        }

        // Align the block vertically
        float ymin, ymax;
        if (IsTopAligned(this.TextAnchor))
        {
            ymin = position.y - cachedBlock.PreferredHeight;
            ymax = position.y;
        }
        else if (IsMiddleAligned(this.TextAnchor))
        {
            ymin = position.y - cachedBlock.PreferredHeight * 0.5f;
            ymax = position.y + cachedBlock.PreferredHeight * 0.5f;
        }
        else if (IsBottomAligned(this.TextAnchor))
        {
            ymin = position.y;
            ymax = position.y + cachedBlock.PreferredHeight;
        }
        else
        {
            throw new InvalidOperationException("Invalid text anchor specified to the typesetter.");
        }

        this.CachedRect = Rect.MinMaxRect(xmin, ymin, xmax, ymax);

        return this.CachedRect;
    }

    public void RenderText(string str, Rect rect, Action<Vector2[]> strokeRenderFunc)
    {
        if (this.Font == null) throw new InvalidOperationException("The typesetter has no font.");

        if (string.IsNullOrEmpty(str)) return;
        if (rect.size == Vector2.zero) return;

        if (cachedBlock == null || cachedString != str)
        {
            ClearCache();

            Vector2 size = rect.size;
            if (size.x < 0 || size.y < 0)
            {
                if (size.x < 0)
                {
                    rect.x += size.x;
                    size.x = -size.x;
                }
                if (size.y < 0)
                {
                    rect.y += size.y;
                    size.y = -size.y;
                }

                rect.size = size;
            }

            CreateCachedBlock(str);
        }

        cachedBlock.Render(strokeRenderFunc, rect);
    }

    public void ClearCache()
    {
        cachedString = null;
        cachedBlock = null;
        cachedRect = null;
    }

    private void CreateCachedBlock(string str)
    {
        BlockElement block = new BlockElement(this.TextAnchor);
        block.Populate(str, this.Font, this.FontScale, this.LineSpacing);
        cachedBlock = block;
        cachedString = str;
    }

    private void OnSettingsChanged()
    {
        if (this.SettingsChanged != null) this.SettingsChanged();
        ClearCache();
    }

    private static IEnumerable<string> GetTextElements(string str)
    {
        var textElementEnumerator = StringInfo.GetTextElementEnumerator(str);

        while (textElementEnumerator.MoveNext())
        {
            yield return textElementEnumerator.GetTextElement();
        }
    }

    private static bool IsWhitespace(string str)
    {
        if (string.IsNullOrEmpty(str)) return false;

        return str.All(char.IsWhiteSpace);
    }

    private static bool IsLeftAligned(TextAnchor anchor)
    {
        return anchor == TextAnchor.UpperLeft || anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.LowerLeft;
    }

    private static bool IsCenterAligned(TextAnchor anchor)
    {
        return anchor == TextAnchor.UpperCenter || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.LowerCenter;
    }

    private static bool IsRightAligned(TextAnchor anchor)
    {
        return anchor == TextAnchor.UpperRight || anchor == TextAnchor.MiddleRight || anchor == TextAnchor.LowerRight;
    }

    private static bool IsTopAligned(TextAnchor anchor)
    {
        return anchor == TextAnchor.UpperLeft || anchor == TextAnchor.UpperCenter || anchor == TextAnchor.UpperRight;
    }

    private static bool IsMiddleAligned(TextAnchor anchor)
    {
        return anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.MiddleRight;
    }

    private static bool IsBottomAligned(TextAnchor anchor)
    {
        return anchor == TextAnchor.LowerLeft || anchor == TextAnchor.LowerCenter || anchor == TextAnchor.LowerRight;
    }

    private static Vector2 GetRectAnchor(Rect rect, TextAnchor anchor)
    {
        return Rect.NormalizedToPoint(rect, Vector2.up);
    }
}