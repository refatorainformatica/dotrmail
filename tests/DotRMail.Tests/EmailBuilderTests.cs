using DotRMail.Core;
using DotRMail.Core.Defaults;
using DotRMail.Core.Models;

namespace DotRMail.Tests;

/// <summary>
/// Tests for the fluent <see cref="Email"/> builder API.
/// </summary>
/// <remarks>
/// Exercises recipient configuration, content rendering, metadata, and send delegation.
/// </remarks>
public class EmailBuilderTests
{
    /// <summary>
    /// Verifies that SetFrom configures the sender address and display name.
    /// </summary>
    /// <remarks>
    /// Uses an in-memory sender to avoid external dependencies.
    /// </remarks>
    [Fact]
    public void SetFrom_ShouldConfigureSenderAddress()
    {
        var email = new Email(new ReplaceRenderer(), new InMemorySender()).SetFrom(
            "from@example.com",
            "Sender"
        );

        Assert.Equal("from@example.com", email.Data.FromAddress?.EmailAddress);
        Assert.Equal("Sender", email.Data.FromAddress?.Name);
    }

    /// <summary>
    /// Verifies that semicolon-delimited To addresses and names are parsed into multiple recipients.
    /// </summary>
    /// <remarks>
    /// Covers the multi-recipient overload of <see cref="Email.To(string, string?)"/>.
    /// </remarks>
    [Fact]
    public void To_WithSemicolonSeparatedAddresses_ShouldAddMultipleRecipients()
    {
        var email = new Email(new ReplaceRenderer(), new InMemorySender()).To(
            "a@example.com;b@example.com",
            "Alice;Bob"
        );

        Assert.Equal(2, email.Data.ToAddresses.Count);
        Assert.Equal("a@example.com", email.Data.ToAddresses[0].EmailAddress);
        Assert.Equal("Alice", email.Data.ToAddresses[0].Name);
        Assert.Equal("b@example.com", email.Data.ToAddresses[1].EmailAddress);
        Assert.Equal("Bob", email.Data.ToAddresses[1].Name);
    }

    /// <summary>
    /// Verifies that CC, BCC, and ReplyTo recipients are added to composed data.
    /// </summary>
    /// <remarks>
    /// Each collection should contain exactly one entry after chaining.
    /// </remarks>
    [Fact]
    public void Cc_Bcc_ReplyTo_ShouldConfigureRecipients()
    {
        var email = new Email(new ReplaceRenderer(), new InMemorySender())
            .CC("cc@example.com", "CC User")
            .BCC("bcc@example.com")
            .ReplyTo("reply@example.com", "Reply User");

        Assert.Single(email.Data.CcAddresses);
        Assert.Single(email.Data.BccAddresses);
        Assert.Single(email.Data.ReplyToAddresses);
    }

    /// <summary>
    /// Verifies that Subject and Body configure message content and HTML flag.
    /// </summary>
    /// <remarks>
    /// HTML bodies must set <see cref="EmailData.IsHtml"/> to true.
    /// </remarks>
    [Fact]
    public void Subject_Body_ShouldConfigureMessageContent()
    {
        var email = new Email(new ReplaceRenderer(), new InMemorySender())
            .Subject("Subject")
            .Body("<p>HTML</p>", isHtml: true);

        Assert.Equal("Subject", email.Data.Subject);
        Assert.Equal("<p>HTML</p>", email.Data.Body);
        Assert.True(email.Data.IsHtml);
    }

    /// <summary>
    /// Verifies that inline templates render through the configured replace renderer.
    /// </summary>
    /// <remarks>
    /// Template methods default HTML output to true when not specified otherwise.
    /// </remarks>
    [Fact]
    public void UsingTemplate_ShouldRenderBodyWithReplaceRenderer()
    {
        var email = new Email(new ReplaceRenderer(), new InMemorySender()).UsingTemplate(
            "Hello ##Name##",
            new { Name = "Maria" }
        );

        Assert.Equal("Hello Maria", email.Data.Body);
        Assert.True(email.Data.IsHtml);
    }

    /// <summary>
    /// Verifies that priority helpers assign high and low delivery priority values.
    /// </summary>
    /// <remarks>
    /// Each builder instance maintains its own <see cref="EmailData.Priority"/> state.
    /// </remarks>
    [Fact]
    public void HighPriority_LowPriority_ShouldSetPriority()
    {
        var high = new Email(new ReplaceRenderer(), new InMemorySender()).HighPriority();
        var low = new Email(new ReplaceRenderer(), new InMemorySender()).LowPriority();

        Assert.Equal(Priority.High, high.Data.Priority);
        Assert.Equal(Priority.Low, low.Data.Priority);
    }

    /// <summary>
    /// Verifies that tags and custom headers are stored on composed email data.
    /// </summary>
    /// <remarks>
    /// Header keys overwrite previous values with the same name.
    /// </remarks>
    [Fact]
    public void Tag_Header_ShouldAddMetadata()
    {
        var email = new Email(new ReplaceRenderer(), new InMemorySender())
            .Tag("welcome")
            .Header("X-Custom", "value");

        Assert.Contains("welcome", email.Data.Tags);
        Assert.Equal("value", email.Data.Headers["X-Custom"]);
    }

    /// <summary>
    /// Verifies that SendAsync delegates to the configured sender implementation.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="InMemorySender"/> to capture the composed message without network I/O.
    /// </remarks>
    [Fact]
    public async Task SendAsync_ShouldUseConfiguredSender()
    {
        var sender = new InMemorySender();
        var email = Email
            .From("from@example.com")
            .To("to@example.com")
            .Subject("Test")
            .Body("Body");

        email.Sender = sender;

        var response = await email.SendAsync();

        Assert.True(response.Successful);
        Assert.Single(sender.SentEmails);
        Assert.Equal("Test", sender.SentEmails[0].Subject);
    }
}
