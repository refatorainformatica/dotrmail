using DotRMail.Core.Models;

namespace DotRMail.Core.Abstractions;

/// <summary>
/// Contract for email sending implementations.
/// </summary>
/// <remarks>
/// Senders receive a composed <see cref="IDotRMail"/> and transmit its <see cref="EmailData"/>.
/// Register custom implementations via dependency injection or assign them on <see cref="Email"/>.
/// </remarks>
public interface ISender
{
    /// <summary>
    /// Sends an email synchronously.
    /// </summary>
    /// <param name="email">Email instance to send.</param>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Implementations should honor cancellation before starting network or I/O work when possible.
    /// </remarks>
    SendResponse Send(IDotRMail email, CancellationToken? token = null);

    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="email">Email instance to send.</param>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Preferred entry point for non-blocking dispatch in application code.
    /// </remarks>
    Task<SendResponse> SendAsync(IDotRMail email, CancellationToken? token = null);
}
