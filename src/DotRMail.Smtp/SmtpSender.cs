using System.Net.Mail;
using System.Text;
using DotRMail.Core;
using DotRMail.Core.Abstractions;
using DotRMail.Core.Models;

namespace DotRMail.Smtp;

/// <summary>
/// SMTP email sender using <see cref="SmtpClient"/>.
/// </summary>
/// <remarks>
/// Builds a <see cref="MailMessage"/> from composed email data and dispatches it through SMTP.
/// Supports factory-created clients, reused clients, and multipart plain-text alternatives.
/// </remarks>
public class SmtpSender : ISender
{
    /// <summary>
    /// Factory used to create a disposable <see cref="SmtpClient"/> per send.
    /// </summary>
    /// <remarks>
    /// Set when the sender is constructed with a factory delegate instead of a shared client.
    /// </remarks>
    private readonly Func<SmtpClient>? _clientFactory;

    /// <summary>
    /// Shared <see cref="SmtpClient"/> instance reused across send operations.
    /// </summary>
    /// <remarks>
    /// When set, the sender does not dispose the client after each send.
    /// </remarks>
    private readonly SmtpClient? _smtpClient;

    /// <summary>
    /// Creates a sender with default SMTP settings.
    /// </summary>
    /// <remarks>
    /// Uses a factory that creates a new default <see cref="SmtpClient"/> for each send.
    /// </remarks>
    public SmtpSender()
        : this(() => new SmtpClient()) { }

    /// <summary>
    /// Creates a sender that creates and disposes an <see cref="SmtpClient"/> for each send.
    /// </summary>
    /// <param name="clientFactory">Factory that produces configured SMTP clients.</param>
    /// <remarks>
    /// Preferred for short-lived clients with explicit host and credential configuration.
    /// </remarks>
    public SmtpSender(Func<SmtpClient> clientFactory)
    {
        _clientFactory = clientFactory;
    }

    /// <summary>
    /// Creates a sender that reuses an existing <see cref="SmtpClient"/> instance.
    /// </summary>
    /// <param name="smtpClient">Shared SMTP client used for all sends.</param>
    /// <remarks>
    /// Caller owns client lifetime and configuration.
    /// </remarks>
    public SmtpSender(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
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
        Task.Run(() => SendAsync(email, token)).GetAwaiter().GetResult();

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
        var response = new SendResponse();
        var message = CreateMailMessage(email);

        if (token?.IsCancellationRequested == true)
        {
            response.ErrorMessages.Add("Send operation was cancelled by the cancellation token.");
            return response;
        }

        if (_smtpClient is null)
        {
            using var client = _clientFactory!();
            await client.SendMailExAsync(message, token ?? CancellationToken.None);
        }
        else
        {
            await _smtpClient.SendMailExAsync(message, token ?? CancellationToken.None);
        }

        return response;
    }

    /// <summary>
    /// Maps composed email data to a <see cref="MailMessage"/>.
    /// </summary>
    /// <param name="email">Composed email instance.</param>
    /// <returns>A configured mail message ready for SMTP dispatch.</returns>
    /// <remarks>
    /// Builds multipart content when a plain-text alternative body is present.
    /// </remarks>
    private static MailMessage CreateMailMessage(IDotRMail email)
    {
        var data = email.Data;
        MailMessage message;

        if (!string.IsNullOrEmpty(data.PlaintextAlternativeBody))
        {
            message = new MailMessage
            {
                Subject = data.Subject,
                Body = data.PlaintextAlternativeBody,
                IsBodyHtml = false,
                From = CreateMailAddress(data.FromAddress),
            };

            var mimeType = new System.Net.Mime.ContentType("text/html; charset=UTF-8");
            var alternate = AlternateView.CreateAlternateViewFromString(
                data.Body ?? string.Empty,
                mimeType
            );
            message.AlternateViews.Add(alternate);
        }
        else
        {
            message = new MailMessage
            {
                Subject = data.Subject,
                Body = data.Body,
                IsBodyHtml = data.IsHtml,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                From = CreateMailAddress(data.FromAddress),
            };
        }

        foreach (var header in data.Headers)
        {
            message.Headers.Add(header.Key, header.Value);
        }

        data.ToAddresses.ForEach(x => message.To.Add(CreateMailAddress(x)));
        data.CcAddresses.ForEach(x => message.CC.Add(CreateMailAddress(x)));
        data.BccAddresses.ForEach(x => message.Bcc.Add(CreateMailAddress(x)));
        data.ReplyToAddresses.ForEach(x => message.ReplyToList.Add(CreateMailAddress(x)));

        message.Priority = data.Priority switch
        {
            Priority.Low => MailPriority.Low,
            Priority.High => MailPriority.High,
            _ => MailPriority.Normal,
        };

        data.Attachments.ForEach(x =>
        {
            var attachment = new System.Net.Mail.Attachment(x.Data!, x.Filename, x.ContentType);
            attachment.ContentId = x.ContentId;
            message.Attachments.Add(attachment);
        });

        return message;
    }

    /// <summary>
    /// Converts a DotRMail address into a <see cref="MailAddress"/>.
    /// </summary>
    /// <param name="address">Source address.</param>
    /// <returns>A mail address with optional display name.</returns>
    /// <remarks>
    /// Throws when the source address is null because SMTP requires a configured sender.
    /// </remarks>
    private static MailAddress CreateMailAddress(Address? address)
    {
        if (address is null)
        {
            throw new InvalidOperationException("Sender address is not configured.");
        }

        return string.IsNullOrEmpty(address.Name)
            ? new MailAddress(address.EmailAddress)
            : new MailAddress(address.EmailAddress, address.Name);
    }
}

/// <summary>
/// Async helpers for legacy <see cref="SmtpClient"/> send operations.
/// </summary>
/// <remarks>
/// Wraps event-based <see cref="SmtpClient.SendAsync(MailMessage, object?)"/> with task-based semantics.
/// </remarks>
internal static class SendMailEx
{
    /// <summary>
    /// Sends a mail message asynchronously with cancellation support.
    /// </summary>
    /// <param name="client">SMTP client performing the send.</param>
    /// <param name="message">Message to send.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that completes when the send finishes.</returns>
    /// <remarks>
    /// Registers <see cref="SmtpClient.SendAsyncCancel"/> when cancellation is requested.
    /// </remarks>
    public static Task SendMailExAsync(
        this SmtpClient client,
        MailMessage message,
        CancellationToken token = default
    )
    {
        return Task.Run(() => SendMailExImplAsync(client, message, token), token);
    }

    /// <summary>
    /// Implements the async send using <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    /// <param name="client">SMTP client performing the send.</param>
    /// <param name="message">Message to send.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that completes when the send finishes.</returns>
    /// <remarks>
    /// Unsubscribes from <see cref="SmtpClient.SendCompleted"/> in all completion paths.
    /// </remarks>
    private static async Task SendMailExImplAsync(
        SmtpClient client,
        MailMessage message,
        CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        var tcs = new TaskCompletionSource<bool>();
        SendCompletedEventHandler? handler = null;
        Action unsubscribe = () => client.SendCompleted -= handler;

        handler = async (_, e) =>
        {
            unsubscribe();
            await Task.Yield();

            if (e.UserState != tcs)
            {
                tcs.TrySetException(
                    new InvalidOperationException("Unexpected state during SMTP send.")
                );
            }
            else if (e.Cancelled)
            {
                tcs.TrySetCanceled(token);
            }
            else if (e.Error is not null)
            {
                tcs.TrySetException(e.Error);
            }
            else
            {
                tcs.TrySetResult(true);
            }
        };

        client.SendCompleted += handler;

        try
        {
            client.SendAsync(message, tcs);
            using (token.Register(client.SendAsyncCancel))
            {
                await tcs.Task.ConfigureAwait(false);
            }
        }
        finally
        {
            unsubscribe();
        }
    }
}
