using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HersheyFont : ScriptableObject
{
    public List<HersheyCharacter> Characters;

    public HersheyCharacter GetCharacterFromString(string characterString)
    {
        return this.Characters.SingleOrDefault(ch => ch.Character == characterString);
    }
}

[Serializable]
public class HersheyCharacter
{
    public string Character;
    public int GlyphNumber;
    public Vector2Int Bounds;
    public List<FontStroke> Strokes;

    public int Width { get { return Mathf.Abs(this.Bounds.x) + Mathf.Abs(this.Bounds.y); } }
}

[Serializable]
public class FontStroke
{
    public List<Vector2Int> Points;
}