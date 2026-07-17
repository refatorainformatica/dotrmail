namespace DotRMail.Core;

/// <summary>
/// Utility extensions for collections.
/// </summary>
/// <remarks>
/// Provides small helpers used by the fluent email API and SMTP sender for collection operations.
/// </remarks>
public static class ListExtensions
{
    /// <summary>
    /// Executes an action for each item in a collection.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="enumerable">Source sequence.</param>
    /// <param name="consumer">Action invoked for each element.</param>
    /// <remarks>
    /// Equivalent to a <c>foreach</c> with an inline delegate; avoids external LINQ dependencies.
    /// </remarks>
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> consumer)
    {
        foreach (var item in enumerable)
        {
            consumer(item);
        }
    }

    /// <summary>
    /// Adds multiple items to a list.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="list">Target list.</param>
    /// <param name="items">Items to append.</param>
    /// <remarks>
    /// Uses the native <see cref="List{T}.AddRange"/> when the list is a concrete <see cref="List{T}"/>.
    /// </remarks>
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    {
        if (list is List<T> concreteList)
        {
            concreteList.AddRange(items);
            return;
        }

        foreach (var item in items)
        {
            list.Add(item);
        }
    }
}
