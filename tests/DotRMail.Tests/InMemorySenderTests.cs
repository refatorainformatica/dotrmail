using DotRMail.Core;
using DotRMail.Core.Defaults;

namespace DotRMail.Tests;

/// <summary>
/// Tests for <see cref="InMemorySender"/> capture and reset behavior.
/// </summary>
/// <remarks>
/// Validates the in-memory sender used as a test double across the suite.
/// </remarks>
public class InMemorySenderTests
{
    /// <summary>
    /// Verifies that SendAsync stores a clone of the composed email data.
    /// </summary>
    /// <remarks>
    /// Captured subject and body must match the values set on the fluent builder.
    /// </remarks>
    [Fact]
    public async Task SendAsync_ShouldCaptureEmailData()
    {
        var sender = new InMemorySender();
        var email = Email
            .From("from@example.com")
            .To("to@example.com")
            .Subject("Subject")
            .Body("Message");

        email.Sender = sender;

        await email.SendAsync();

        Assert.Single(sender.SentEmails);
        Assert.Equal("Subject", sender.SentEmails[0].Subject);
        Assert.Equal("Message", sender.SentEmails[0].Body);
    }

    /// <summary>
    /// Verifies that Clear removes all previously captured emails.
    /// </summary>
    /// <remarks>
    /// Supports resetting sender state between test cases.
    /// </remarks>
    [Fact]
    public void Clear_ShouldRemoveCapturedEmails()
    {
        var sender = new InMemorySender();
        var email = Email.From("from@example.com").To("to@example.com").Subject("Test");
        email.Sender = sender;

        email.Send();
        sender.Clear();

        Assert.Empty(sender.SentEmails);
    }
}
