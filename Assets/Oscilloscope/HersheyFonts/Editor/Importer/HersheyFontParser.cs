using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class HersheyFontParser : IDisposable
{
    private TextReader _textReader;

    private static readonly Regex GlyphInfoRegex = new Regex("^(?<GlyphNo>(\\ |\\d){5})(?<PairCount>(\\ |\\d){3})(?<Bounds>\\D{2})(?<Instructions>\\D*)$", RegexOptions.ExplicitCapture);

    private const char CoordinateOriginCharacter = 'R';

    private static readonly string PenUpSequence = new string(new[] { ' ', CoordinateOriginCharacter });

    public HersheyFontParser(TextReader textReader)
    {
        if (textReader == null) throw new ArgumentNullException("textReader");

        _textReader = textReader;
    }

    public bool CanRead { get { return _textReader.Peek() >= 0; } }

    public IEnumerable<HersheyCharacter> ReadCharacters()
    {
        bool isReadingCharacter = false;
        int glyphNumber = 31;
        int pairCount = 0;
        List<FontStroke> strokes = new List<FontStroke>();
        Vector2Int bounds = new Vector2Int();
        StringBuilder instructions = new StringBuilder();
        while (this.CanRead)
        {
            string line = _textReader.ReadLine();

            Match match = GlyphInfoRegex.Match(line);

            if (match.Success)
            {
                if (isReadingCharacter)
                {
                    // new character so yield the complete one
                    strokes = new List<FontStroke>(GetFontStrokesFromEncodedString(instructions.ToString()));
                    yield return new HersheyCharacter
                    {
                        GlyphNumber = glyphNumber++,
                        Character = new string((char)glyphNumber, 1),
                        Bounds = bounds,
                        Strokes = strokes
                    };
                }

                // get the new character information from the first line of its data
                //glyphNumber = int.Parse(match.Groups["GlyphNo"].Value.Replace(" ", string.Empty));
                pairCount = int.Parse(match.Groups["PairCount"].Value.Replace(" ", string.Empty));

                bounds = GetPointsFromEncodedString(match.Groups["Bounds"].Value).First();

                instructions = new StringBuilder(match.Groups["Instructions"].Value);

                isReadingCharacter = true;
            }
            else if (isReadingCharacter)
            {
                instructions.Append(line);
            }
        }
    }
    
    private static IEnumerable<FontStroke> GetFontStrokesFromEncodedString(string str)
    {
        string[] strokes = str.Split(new[] { PenUpSequence }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string stroke in strokes)
        {
            yield return new FontStroke { Points = new List<Vector2Int>(GetPointsFromEncodedString(stroke)) };
        }
    }

    private static IEnumerable<Vector2Int> GetPointsFromEncodedString(string str)
    {
        if (str.Contains(PenUpSequence)) throw new ArgumentException(string.Format("The string contains the pen up sequence \"{0}\".", PenUpSequence));

        for (int i = 1; i < str.Length; i += 2)
        {
            int x = str[i - 1] - CoordinateOriginCharacter;
            int y = CoordinateOriginCharacter - str[i];

            yield return new Vector2Int(x, y);
        }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _textReader.Dispose();
            }
            _textReader = null;

            disposedValue = true;
        }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
    }
    #endregion

}
