using DotRMail.Core.Abstractions;
using DotRMail.Core.Models;

namespace DotRMail.Core.Defaults;

/// <summary>
/// Sender that persists emails to disk for development and debugging.
/// </summary>
/// <remarks>
/// Writes a simplified text representation of each message to the configured directory.
/// </remarks>
public class SaveToDiskSender : ISender
{
    /// <summary>
    /// Target directory where email files are written.
    /// </summary>
    /// <remarks>
    /// Created automatically during construction when it does not exist.
    /// </remarks>
    private readonly string _directory;

    /// <summary>
    /// Creates a sender that saves emails to the specified directory.
    /// </summary>
    /// <param name="directory">Target directory.</param>
    /// <remarks>
    /// Ensures the directory exists before the first send operation.
    /// </remarks>
    public SaveToDiskSender(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(_directory);
    }

    /// <summary>
    /// Sends an email synchronously.
    /// </summary>
    /// <param name="email">Email instance to send.</param>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Implementations should honor cancellation before starting network or I/O work when possible.
    /// </remarks>
    public SendResponse Send(IDotRMail email, CancellationToken? token = null) =>
        SendAsync(email, token).GetAwaiter().GetResult();

    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="email">Email instance to send.</param>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Preferred entry point for non-blocking dispatch in application code.
    /// </remarks>
    public async Task<SendResponse> SendAsync(IDotRMail email, CancellationToken? token = null)
    {
        token?.ThrowIfCancellationRequested();

        var response = new SendResponse();
        var filename = Path.Combine(
            _directory,
            $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Random.Shared.Next(1000, 9999)}.eml"
        );

        await using var writer = new StreamWriter(File.OpenWrite(filename));

        var from = email.Data.FromAddress;
        await writer.WriteLineAsync($"From: {from}");
        await writer.WriteLineAsync($"To: {string.Join(", ", email.Data.ToAddresses)}");
        await writer.WriteLineAsync($"Cc: {string.Join(", ", email.Data.CcAddresses)}");
        await writer.WriteLineAsync($"Bcc: {string.Join(", ", email.Data.BccAddresses)}");
        await writer.WriteLineAsync($"ReplyTo: {string.Join(", ", email.Data.ReplyToAddresses)}");
        await writer.WriteLineAsync($"Subject: {email.Data.Subject}");
        await writer.WriteLineAsync($"Priority: {email.Data.Priority}");
        await writer.WriteLineAsync($"IsHtml: {email.Data.IsHtml}");

        foreach (var header in email.Data.Headers)
        {
            await writer.WriteLineAsync($"{header.Key}: {header.Value}");
        }

        await writer.WriteLineAsync();
        await writer.WriteAsync(email.Data.Body ?? string.Empty);

        if (!string.IsNullOrEmpty(email.Data.PlaintextAlternativeBody))
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("--- Plaintext Alternative ---");
            await writer.WriteAsync(email.Data.PlaintextAlternativeBody);
        }

        response.MessageId = filename;
        return response;
    }
}
