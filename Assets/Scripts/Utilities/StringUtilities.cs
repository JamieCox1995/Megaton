using System;
using System.Collections.Generic;
using System.Text;

public static class StringUtilities
{
    public static string AllToString<T>(this IEnumerable<T> collection)
    {
        var sb = new StringBuilder();

        foreach (var item in collection)
        {
            sb.Append(item.ToString());
        }

        return sb.ToString();
    }

    public static string AllToString<T>(this IEnumerable<T> collection, Func<T, string> stringFunc)
    {
        var sb = new StringBuilder();

        foreach (var item in collection)
        {
            sb.Append(stringFunc(item));
        }

        return sb.ToString();
    }
}
