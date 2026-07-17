using DotRMail.Core;
using DotRMail.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// DI extensions for configuring DotRMail.
/// </summary>
/// <remarks>
/// Entry point for registering the fluent email builder, factory, and optional renderer or sender packages.
/// </remarks>
public static class DotRMailServiceCollectionExtensions
{
    /// <summary>
    /// Registers DotRMail services with a default sender address.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="defaultFromEmail">Default sender email address.</param>
    /// <param name="defaultFromName">Default sender display name.</param>
    /// <returns>Builder for additional renderer and sender configuration.</returns>
    /// <remarks>
    /// Registers transient <see cref="IDotRMail"/> and <see cref="IDotRMailFactory"/> services.
    /// Renderer and sender fall back to <see cref="Email.DefaultRenderer"/> and <see cref="Email.DefaultSender"/>.
    /// </remarks>
    public static DotRMailServicesBuilder AddDotRMail(
        this IServiceCollection services,
        string defaultFromEmail,
        string defaultFromName = ""
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new DotRMailServicesBuilder(services);

        services.TryAdd(
            ServiceDescriptor.Transient<IDotRMail>(provider => new Email(
                provider.GetService<ITemplateRenderer>() ?? Email.DefaultRenderer,
                provider.GetService<ISender>() ?? Email.DefaultSender,
                defaultFromEmail,
                defaultFromName
            ))
        );

        services.TryAddTransient<IDotRMailFactory, DotRMailFactory>();

        return builder;
    }
}

/// <summary>
/// Builder for chaining DotRMail configuration.
/// </summary>
/// <remarks>
/// Returned by <see cref="DotRMailServiceCollectionExtensions.AddDotRMail"/> to register renderers and senders fluently.
/// </remarks>
public class DotRMailServicesBuilder
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    /// <remarks>
    /// Passed to extension methods in Liquid and SMTP packages for additional registration.
    /// </remarks>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new builder for the given service collection.
    /// </summary>
    /// <param name="services">Service collection to configure.</param>
    /// <remarks>
    /// Internal constructor; instances are created only by <see cref="DotRMailServiceCollectionExtensions.AddDotRMail"/>.
    /// </remarks>
    internal DotRMailServicesBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
