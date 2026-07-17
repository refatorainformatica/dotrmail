namespace DotRMail.Core.Abstractions;

/// <summary>
/// Factory for creating independent <see cref="IDotRMail"/> instances.
/// </summary>
/// <remarks>
/// Registered in dependency injection so scoped or transient email builders can be resolved
/// without sharing mutable state between requests.
/// </remarks>
public interface IDotRMailFactory
{
    /// <summary>
    /// Creates a new email instance with configured dependencies.
    /// </summary>
    /// <returns>A new <see cref="IDotRMail"/> resolved from the service provider.</returns>
    /// <remarks>
    /// Each call returns a fresh instance suitable for composing a single message.
    /// </remarks>
    IDotRMail Create();
}
