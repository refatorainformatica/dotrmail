namespace DotRMail.Core.Models;

/// <summary>
/// Represents the result of an email send operation.
/// </summary>
/// <remarks>
/// Returned by <see cref="Abstractions.ISender"/> implementations after a send attempt.
/// Check <see cref="Successful"/> before relying on <see cref="MessageId"/>.
/// </remarks>
public class SendResponse
{
    /// <summary>
    /// Gets or sets the message identifier returned by the provider, when available.
    /// </summary>
    /// <remarks>
    /// May contain a provider message id or a local artifact path depending on the sender.
    /// </remarks>
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the list of error messages that occurred during sending.
    /// </summary>
    /// <remarks>
    /// An empty list indicates success; multiple entries may be present for compound failures.
    /// </remarks>
    public IList<string> ErrorMessages { get; set; } = new List<string>();

    /// <summary>
    /// Gets a value indicating whether the send operation succeeded.
    /// </summary>
    /// <remarks>
    /// Derived from <see cref="ErrorMessages"/>; no errors means success.
    /// </remarks>
    public bool Successful => ErrorMessages.Count == 0;
}

/// <summary>
/// Represents a send result with additional provider-specific data.
/// </summary>
/// <typeparam name="T">Additional data type returned by the provider.</typeparam>
/// <remarks>
/// Extends <see cref="SendResponse"/> when a sender exposes structured provider metadata.
/// </remarks>
public class SendResponse<T> : SendResponse
{
    /// <summary>
    /// Gets or sets additional data returned by the provider.
    /// </summary>
    /// <remarks>
    /// Populated only by senders that surface typed provider responses.
    /// </remarks>
    public T? Data { get; set; }
}
