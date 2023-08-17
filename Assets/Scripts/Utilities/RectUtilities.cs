using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class RectUtilities
{
    public static bool ClipLine(this Rect rect, ref Vector2 start, ref Vector2 end)
    {
        // Using the Cohen-Sutherland algorithm:
        // https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm

        var startFlags = GetLineClippingFlags(rect, start);
        var endFlags = GetLineClippingFlags(rect, end);

        while (true)
        {
            if (startFlags == LineClippingFlags.Inside && endFlags == LineClippingFlags.Inside)
            {
                // Both points are inside the rect so trivially return true.
                return true;
            }
            else if ((startFlags & endFlags) != 0)
            {
                // Both points share an outer zone (LeftOfRect, RightOfRect, BelowRect, or AboveRect)
                // so must necessarily be entirely outside the rect.
                return false;
            }
            else
            {
                LineClippingFlags flags = startFlags != LineClippingFlags.Default ? startFlags : endFlags;

                Vector2 v;
                if (flags.HasFlag(LineClippingFlags.AboveRect))
                {
                    float x = start.x + (end.x - start.x) * (rect.yMax - start.y) / (end.y - start.y);
                    float y = rect.yMax;

                    v = new Vector2(x, y);
                }
                else if (flags.HasFlag(LineClippingFlags.BelowRect))
                {
                    float x = start.x + (end.x - start.x) * (rect.yMin - start.y) / (end.y - start.y);
                    float y = rect.yMin;

                    v = new Vector2(x, y);
                }
                else if (flags.HasFlag(LineClippingFlags.RightOfRect))
                {
                    float x = rect.xMax;
                    float y = start.y + (end.y - start.y) * (rect.xMax - start.x) / (end.x - start.x);

                    v = new Vector2(x, y);
                }
                else if (flags.HasFlag(LineClippingFlags.LeftOfRect))
                {
                    float x = rect.xMin;
                    float y = start.y + (end.y - start.y) * (rect.xMin - start.x) / (end.x - start.x);

                    v = new Vector2(x, y);
                }
                else
                {
                    throw new InvalidOperationException("Invalid line clipping flags.");
                }

                if (flags == startFlags)
                {
                    start = v;
                    startFlags = GetLineClippingFlags(rect, v);
                }
                else
                {
                    end = v;
                    endFlags = GetLineClippingFlags(rect, v);
                }
            }
        }
    }

    private static LineClippingFlags GetLineClippingFlags(Rect rect, Vector2 v)
    {
        LineClippingFlags result = LineClippingFlags.Default;

        if (v.x < rect.xMin)
        {
            result |= LineClippingFlags.LeftOfRect;
        }
        else if (v.x > rect.xMax)
        {
            result |= LineClippingFlags.RightOfRect;
        }

        if (v.y < rect.yMin)
        {
            result |= LineClippingFlags.BelowRect;
        }
        else if (v.y > rect.yMax)
        {
            result |= LineClippingFlags.AboveRect;
        }

        return result;
    }

    [Flags]
    private enum LineClippingFlags
    {
        Default = 0x00,
        Inside = 0x00,

        LeftOfRect = 0x01,
        RightOfRect = 0x02,
        BelowRect = 0x04,
        AboveRect = 0x08,

        TopLeft = LeftOfRect | AboveRect,
        TopCenter = AboveRect,
        TopRight = RightOfRect | AboveRect,
        MiddleLeft = LeftOfRect,
        MiddleCenter = Inside,
        MiddleRight = RightOfRect,
        BottomLeft = LeftOfRect | BelowRect,
        BottomCenter = BelowRect,
        BottomRight = RightOfRect | BelowRect,
    }
}
