using UnityEngine;

public static class UnityObjectUtils
{
    /// <summary>
    /// Returns whether this <see cref="UnityEngine.Object"/> has been marked as destroyed or missing by the Unity engine.
    /// </summary>
    /// <param name="obj">The <see cref="UnityEngine.Object"/> to check.</param>
    /// <returns><c>true</c> if the object has been marked as destroyed or missing; otherwise, <c>false</c>.</returns>
    public static bool IsDestroyedOrMissing(this UnityEngine.Object obj)
    {
        return obj == null && obj.Exists();
    }

    /// <summary>
    /// Returns whether this <see cref="UnityEngine.Object"/> exists in memory.
    /// </summary>
    /// <param name="obj">The <see cref="UnityEngine.Object"/> to check.</param>
    /// <returns><c>true</c> if the object exists in memory; otherwise, <c>false</c>.</returns>
    public static bool Exists(this UnityEngine.Object obj)
    {
        return !Object.ReferenceEquals(obj, null);
    }
}
