using System;
using System.Collections.Generic;

public static class RandomUtils
{
    public static void ShuffleInPlace<T>(this T[] array)
    {
        for (int n = array.Length; n > 1; n--)
        {
            int i = UnityEngine.Random.Range(0, n);

            T temp = array[i];
            array[i] = array[n - 1];
            array[n - 1] = temp;
        }
    }

    public static void ShuffleInPlace<T>(this IList<T> list)
    {
        for (int n = list.Count; n > 1; n--)
        {
            int i = UnityEngine.Random.Range(0, n);

            T temp = list[i];
            list[i] = list[n - 1];
            list[n - 1] = temp;
        }
    }

    public static T[] Shuffle<T>(this T[] array)
    {
        T[] result = new T[array.Length];
        Array.Copy(array, result, array.Length);

        result.ShuffleInPlace();

        return result;
    }

    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        List<T> result = new List<T>(list);

        result.ShuffleInPlace();

        return result;
    }
}