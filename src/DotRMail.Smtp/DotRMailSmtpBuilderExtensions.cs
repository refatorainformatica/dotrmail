using DotRMail.Core.Abstractions;
using DotRMail.Smtp;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// DI extensions for the DotRMail SMTP provider.
/// </summary>
/// <remarks>
/// Registers <see cref="SmtpSender"/> as the default <see cref="ISender"/> implementation.
/// </remarks>
public static class DotRMailSmtpBuilderExtensions
{
    /// <summary>
    /// Registers the SMTP provider as the default <see cref="ISender"/>.
    /// </summary>
    /// <param name="builder">DotRMail services builder.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <remarks>
    /// Uses default <see cref="System.Net.Mail.SmtpClient"/> settings from the runtime environment.
    /// </remarks>
    public static DotRMailServicesBuilder AddSmtpSender(this DotRMailServicesBuilder builder)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ISender, SmtpSender>());
        return builder;
    }

    /// <summary>
    /// Registers the SMTP provider with a custom <see cref="System.Net.Mail.SmtpClient"/> factory.
    /// </summary>
    /// <param name="builder">DotRMail services builder.</param>
    /// <param name="clientFactory">Factory that creates configured SMTP clients.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <remarks>
    /// Useful when credentials, SSL, or timeout settings must be applied per client instance.
    /// </remarks>
    public static DotRMailServicesBuilder AddSmtpSender(
        this DotRMailServicesBuilder builder,
        Func<System.Net.Mail.SmtpClient> clientFactory
    )
    {
        builder.Services.TryAdd(
            ServiceDescriptor.Singleton<ISender>(_ => new SmtpSender(clientFactory))
        );
        return builder;
    }

    /// <summary>
    /// Registers the SMTP provider with host and port.
    /// </summary>
    /// <param name="builder">DotRMail services builder.</param>
    /// <param name="host">SMTP server host name.</param>
    /// <param name="port">SMTP server port.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <remarks>
    /// Creates a new <see cref="System.Net.Mail.SmtpClient"/> for each send using the supplied endpoint.
    /// </remarks>
    public static DotRMailServicesBuilder AddSmtpSender(
        this DotRMailServicesBuilder builder,
        string host,
        int port = 25
    )
    {
        builder.Services.TryAdd(
            ServiceDescriptor.Singleton<ISender>(_ => new SmtpSender(() =>
                new System.Net.Mail.SmtpClient(host, port)
            ))
        );

        return builder;
    }
}
