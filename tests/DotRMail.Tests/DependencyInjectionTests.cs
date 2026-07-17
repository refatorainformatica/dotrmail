using DotRMail.Core;
using DotRMail.Core.Abstractions;
using DotRMail.Core.Defaults;
using DotRMail.Liquid;
using DotRMail.Smtp;
using Microsoft.Extensions.DependencyInjection;

namespace DotRMail.Tests;

/// <summary>
/// Tests for DotRMail dependency injection registration extensions.
/// </summary>
/// <remarks>
/// Validates core service registration and optional Liquid and SMTP package integration.
/// </remarks>
public class DependencyInjectionTests
{
    /// <summary>
    /// Verifies that AddDotRMail registers email services and default from address.
    /// </summary>
    /// <remarks>
    /// Asserts both <see cref="IDotRMail"/> and <see cref="IDotRMailFactory"/> resolve successfully.
    /// </remarks>
    [Fact]
    public void AddDotRMail_ShouldRegisterEmailServices()
    {
        var services = new ServiceCollection();
        services.AddDotRMail("noreply@example.com", "DotRMail");

        var provider = services.BuildServiceProvider();
        var email = provider.GetRequiredService<IDotRMail>();
        var factory = provider.GetRequiredService<IDotRMailFactory>();

        Assert.NotNull(email);
        Assert.NotNull(factory);
        Assert.Equal("noreply@example.com", email.Data.FromAddress?.EmailAddress);
    }

    /// <summary>
    /// Verifies that AddSmtpSender registers <see cref="SmtpSender"/> as <see cref="ISender"/>.
    /// </summary>
    /// <remarks>
    /// Uses host and port overload to configure the SMTP client factory.
    /// </remarks>
    [Fact]
    public void AddSmtpSender_ShouldRegisterSmtpSender()
    {
        var services = new ServiceCollection();
        services.AddDotRMail("noreply@example.com").AddSmtpSender("localhost", 25);

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        Assert.IsType<SmtpSender>(sender);
    }

    /// <summary>
    /// Verifies that AddLiquidRenderer registers <see cref="LiquidRenderer"/> as <see cref="ITemplateRenderer"/>.
    /// </summary>
    /// <remarks>
    /// Confirms the Liquid package replaces the default replace-based renderer in DI.
    /// </remarks>
    [Fact]
    public void AddLiquidRenderer_ShouldRegisterLiquidRenderer()
    {
        var services = new ServiceCollection();
        services.AddDotRMail("noreply@example.com").AddLiquidRenderer();

        var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<ITemplateRenderer>();

        Assert.IsType<LiquidRenderer>(renderer);
    }

    /// <summary>
    /// Verifies that the factory creates a usable email instance with configured defaults.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="InMemorySender"/> to avoid external I/O during the test.
    /// </remarks>
    [Fact]
    public void Factory_Create_ShouldReturnNewEmailInstance()
    {
        var services = new ServiceCollection();
        services.AddDotRMail("noreply@example.com");
        services.AddSingleton<ISender, InMemorySender>();

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDotRMailFactory>();

        var email = factory.Create();
        email.To("user@example.com").Subject("Factory");

        Assert.Equal("noreply@example.com", email.Data.FromAddress?.EmailAddress);
        Assert.Equal("Factory", email.Data.Subject);
    }
}
