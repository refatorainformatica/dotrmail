using System.ComponentModel;

namespace DotRMail.Core.Abstractions;

/// <summary>
/// Hides inherited <see cref="object"/> members from the fluent interface to improve IntelliSense.
/// </summary>
/// <remarks>
/// Applied to fluent APIs such as <see cref="IDotRMail"/> so common object methods do not appear
/// in completion lists and break method chaining discoverability.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IHideObjectMembers
{
    /// <summary>
    /// Returns the runtime type of the current instance.
    /// </summary>
    /// <returns>The runtime type of the current instance.</returns>
    /// <remarks>
    /// Hidden from editor browsing to keep the fluent surface focused on email operations.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new Type GetType();

    /// <summary>
    /// Returns a string representation of the current instance.
    /// </summary>
    /// <returns>A string representation of the current instance.</returns>
    /// <remarks>
    /// Hidden from editor browsing to avoid accidental use during fluent composition.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new string? ToString();

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> when equal; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Hidden from editor browsing because fluent callers rarely need reference equality checks.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new bool Equals(object? obj);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    /// <remarks>
    /// Hidden from editor browsing to reduce noise in fluent API completion.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new int GetHashCode();
}
