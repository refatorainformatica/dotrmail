using DotRMail.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DotRMail.Core;

/// <summary>
/// <see cref="IDotRMailFactory"/> implementation based on dependency injection.
/// </summary>
/// <remarks>
/// Resolves transient <see cref="IDotRMail"/> instances from the configured service provider.
/// </remarks>
public class DotRMailFactory : IDotRMailFactory
{
    /// <summary>
    /// Holds the service provider used to resolve email instances.
    /// </summary>
    /// <remarks>
    /// Captured at construction and reused for every factory call.
    /// </remarks>
    private readonly IServiceProvider _services;

    /// <summary>
    /// Initializes the factory with the service provider.
    /// </summary>
    /// <param name="services">Provider that supplies configured DotRMail services.</param>
    /// <remarks>
    /// Registered as a transient factory alongside <see cref="IDotRMail"/>.
    /// </remarks>
    public DotRMailFactory(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    /// Creates a new email instance with configured dependencies.
    /// </summary>
    /// <returns>A new <see cref="IDotRMail"/> resolved from the service provider.</returns>
    /// <remarks>
    /// Each call returns a fresh instance suitable for composing a single message.
    /// </remarks>
    public IDotRMail Create() => _services.GetRequiredService<IDotRMail>();
}
