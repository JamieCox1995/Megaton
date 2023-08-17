using System.Collections.Generic;
using UnityEngine;

public static class TransformUtilities
{
    /// <summary>
    /// Gets the ancestor of the specified transform according to the given distance.
    /// </summary>
    /// <param name="transform">The <see cref="Transform"/> to get the ancestor for.</param>
    /// <param name="distance">The number of steps in the hierarchy between the specified transform and the desired ancestor.</param>
    /// <returns>A <see cref="Transform"/> object that is the specified number of steps above the given transform in the hierarchy, or the root transform.</returns>
    public static Transform GetAncestor(this Transform transform, int distance)
    {
        while (distance > 0)
        {
            if (transform.parent == null) break;
            transform = transform.parent;
            distance--;
        }

        return transform;
    }

    /// <summary>
    /// Performs a search starting from the specified transform, backtracking when a component of type
    /// <typeparamref name="TComponent"/> or a leaf transform is encountered, and returns the traversed
    /// transforms.
    /// </summary>
    /// <typeparam name="TComponent">The component type to search for.</typeparam>
    /// <param name="transform">The root transform of the search hierarchy.</param>
    /// <param name="ignoreRoot">If set to <c>true</c> then the search will continue if the root contains a
    /// <typeparamref name="TComponent"/> component.</param>
    /// <returns>An array of <see cref="Transform"/> objects that were traversed during the search.</returns>
    public static Transform[] GetSearchHierarchy<TComponent>(this Transform transform, bool ignoreRoot = true)
        where TComponent : class
    {
        List<Transform> result = new List<Transform>();

        Queue<Transform> transformQueue = new Queue<Transform>();
        transformQueue.Enqueue(transform);

        while (transformQueue.Count > 0)
        {
            Transform t = transformQueue.Dequeue();

            result.Add(t);

            TComponent component = t.GetComponent<TComponent>();

            if (component == null || ignoreRoot)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    transformQueue.Enqueue(t.GetChild(i));
                }

                if (ignoreRoot) ignoreRoot = false;
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Gets all instances of the type <typeparamref name="TComponent"/> attached to the given collection of transforms.
    /// </summary>
    /// <typeparam name="TComponent">The component type to retrieve.</typeparam>
    /// <param name="transforms">The collection of transforms to retrieve the components from.</param>
    /// <returns>A collection of <typeparamref name="TComponent"/> objects attached to the given collection of transforms.</returns>
    public static IEnumerable<TComponent> GetAllComponents<TComponent>(this IEnumerable<Transform> transforms)
    {
        List<TComponent> result = new List<TComponent>();

        foreach (Transform t in transforms)
        {
            result.AddRange(t.GetComponents<TComponent>());
        }

        return result;
    }
}

