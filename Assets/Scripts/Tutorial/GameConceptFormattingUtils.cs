using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class GameConceptFormattingUtils
{
    /// <summary>
    /// Matches strings that contain substrings like "<concept>some game concept</concept>".
    /// </summary>
    private static Regex GameConceptRegex = new Regex(@"<concept>(?<GameConcept>\S.*?)</concept>", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

    public static string FormatGameConcepts(this string str)
    {
        return GameConceptRegex.Replace(str, ReplaceMatchWithGameConcept);
    }

    public static string FormatGameConcepts(this string str, string format)
    {
        return GameConceptRegex.Replace(str, match => ReplaceMatchWithFormattedGameConcept(match, format));
    }

    private static string ReplaceMatchWithGameConcept(Match match)
    {
        return ReplaceMatchWithFormattedGameConcept(match, null);
    }

    private static string ReplaceMatchWithFormattedGameConcept(Match match, string format)
    {
        string gameConcept = match.Groups["GameConcept"].Value;

        if (string.IsNullOrEmpty(format)) return gameConcept;

        return string.Format(format, gameConcept);
    }
}
