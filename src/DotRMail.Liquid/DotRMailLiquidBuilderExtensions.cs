using DotRMail.Core.Abstractions;
using DotRMail.Liquid;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// DI extensions for the DotRMail Liquid renderer.
/// </summary>
/// <remarks>
/// Registers <see cref="LiquidRenderer"/> and its options alongside the core DotRMail builder.
/// </remarks>
public static class DotRMailLiquidBuilderExtensions
{
    /// <summary>
    /// Registers the Liquid renderer as the default <see cref="ITemplateRenderer"/>.
    /// </summary>
    /// <param name="builder">DotRMail services builder.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <remarks>
    /// Uses default <see cref="LiquidRendererOptions"/> when no custom configuration is supplied.
    /// </remarks>
    public static DotRMailServicesBuilder AddLiquidRenderer(this DotRMailServicesBuilder builder)
    {
        builder.Services.TryAdd(ServiceDescriptor.Singleton<ITemplateRenderer, LiquidRenderer>());
        builder.Services.TryAdd(
            ServiceDescriptor.Singleton(_ => Options.Options.Create(new LiquidRendererOptions()))
        );
        return builder;
    }

    /// <summary>
    /// Registers the Liquid renderer with custom options.
    /// </summary>
    /// <param name="builder">DotRMail services builder.</param>
    /// <param name="configure">Callback that mutates renderer options before registration.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <remarks>
    /// Options are captured once at registration time and reused for the singleton renderer.
    /// </remarks>
    public static DotRMailServicesBuilder AddLiquidRenderer(
        this DotRMailServicesBuilder builder,
        Action<LiquidRendererOptions> configure
    )
    {
        var options = new LiquidRendererOptions();
        configure(options);

        builder.Services.TryAdd(ServiceDescriptor.Singleton<ITemplateRenderer, LiquidRenderer>());
        builder.Services.TryAdd(ServiceDescriptor.Singleton(_ => Options.Options.Create(options)));

        return builder;
    }
}
