using DotRMail.Core.Abstractions;
using DotRMail.Core.Models;

namespace DotRMail.Core.Defaults;

/// <summary>
/// In-memory sender ideal for unit and integration tests.
/// </summary>
/// <remarks>
/// Captures sent messages in <see cref="SentEmails"/> without network or disk I/O.
/// Thread-safe captures use an internal lock.
/// </remarks>
public class InMemorySender : ISender
{
    /// <summary>
    /// Synchronizes access to the captured email list.
    /// </summary>
    /// <remarks>
    /// Prevents concurrent send operations from corrupting <see cref="SentEmails"/>.
    /// </remarks>
    private readonly object _lock = new();

    /// <summary>
    /// Gets the list of sent emails captured in memory.
    /// </summary>
    /// <remarks>
    /// Each entry is a shallow clone of the composed <see cref="EmailData"/> at send time.
    /// </remarks>
    public IList<EmailData> SentEmails { get; } = new List<EmailData>();

    /// <summary>
    /// Sends an email synchronously.
    /// </summary>
    /// <param name="email">Email instance to send.</param>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Implementations should honor cancellation before starting network or I/O work when possible.
    /// </remarks>
    public SendResponse Send(IDotRMail email, CancellationToken? token = null)
    {
        token?.ThrowIfCancellationRequested();
        Capture(email.Data);
        return new SendResponse { MessageId = Guid.NewGuid().ToString("N") };
    }

    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="email">Email instance to send.</param>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Preferred entry point for non-blocking dispatch in application code.
    /// </remarks>
    public Task<SendResponse> SendAsync(IDotRMail email, CancellationToken? token = null)
    {
        token?.ThrowIfCancellationRequested();
        Capture(email.Data);
        return Task.FromResult(new SendResponse { MessageId = Guid.NewGuid().ToString("N") });
    }

    /// <summary>
    /// Removes all captured emails.
    /// </summary>
    /// <remarks>
    /// Useful between test cases to reset sender state.
    /// </remarks>
    public void Clear()
    {
        lock (_lock)
        {
            SentEmails.Clear();
        }
    }

    /// <summary>
    /// Stores a cloned copy of the provided email data.
    /// </summary>
    /// <param name="data">Composed email data to capture.</param>
    /// <remarks>
    /// Cloning prevents later fluent mutations from altering captured test assertions.
    /// </remarks>
    private void Capture(EmailData data)
    {
        lock (_lock)
        {
            SentEmails.Add(Clone(data));
        }
    }

    /// <summary>
    /// Creates a shallow copy of email data for test inspection.
    /// </summary>
    /// <param name="source">Source email data.</param>
    /// <returns>A new <see cref="EmailData"/> instance with copied collections and scalar fields.</returns>
    /// <remarks>
    /// Attachment streams and deep object graphs are not duplicated.
    /// </remarks>
    private static EmailData Clone(EmailData source) =>
        new()
        {
            FromAddress = source.FromAddress is null
                ? null
                : new Address(source.FromAddress.EmailAddress, source.FromAddress.Name),
            Subject = source.Subject,
            Body = source.Body,
            PlaintextAlternativeBody = source.PlaintextAlternativeBody,
            IsHtml = source.IsHtml,
            Priority = source.Priority,
            ToAddresses = source
                .ToAddresses.Select(a => new Address(a.EmailAddress, a.Name))
                .ToList(),
            CcAddresses = source
                .CcAddresses.Select(a => new Address(a.EmailAddress, a.Name))
                .ToList(),
            BccAddresses = source
                .BccAddresses.Select(a => new Address(a.EmailAddress, a.Name))
                .ToList(),
            ReplyToAddresses = source
                .ReplyToAddresses.Select(a => new Address(a.EmailAddress, a.Name))
                .ToList(),
            Tags = source.Tags.ToList(),
            Headers = new Dictionary<string, string>(source.Headers),
        };
}
