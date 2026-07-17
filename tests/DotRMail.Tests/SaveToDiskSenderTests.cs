using DotRMail.Core;
using DotRMail.Core.Defaults;

namespace DotRMail.Tests;

/// <summary>
/// Tests for <see cref="SaveToDiskSender"/> file persistence behavior.
/// </summary>
/// <remarks>
/// Uses a temporary directory and deletes it after the test completes.
/// </remarks>
public class SaveToDiskSenderTests
{
    /// <summary>
    /// Verifies that SendAsync writes composed email content to disk.
    /// </summary>
    /// <remarks>
    /// MessageId should contain the path to the persisted file, which must include subject and body text.
    /// </remarks>
    [Fact]
    public async Task SendAsync_ShouldPersistEmailToDisk()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"dotrmail-tests-{Guid.NewGuid():N}");
        var sender = new SaveToDiskSender(directory);

        var email = Email
            .From("from@example.com", "Sender")
            .To("to@example.com", "Recipient")
            .Subject("Persistence")
            .Body("Email content");

        email.Sender = sender;

        var response = await email.SendAsync();

        Assert.True(response.Successful);
        Assert.NotNull(response.MessageId);
        Assert.True(File.Exists(response.MessageId));

        var content = await File.ReadAllTextAsync(response.MessageId);
        Assert.Contains("Subject: Persistence", content);
        Assert.Contains("Email content", content);

        Directory.Delete(directory, recursive: true);
    }
}
