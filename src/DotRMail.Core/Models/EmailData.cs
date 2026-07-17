namespace DotRMail.Core.Models;

/// <summary>
/// Contains all data required to compose and send an email.
/// </summary>
/// <remarks>
/// Mutable state object populated by the fluent <see cref="DotRMail.Core.Email"/> API
/// and passed to <see cref="Abstractions.ISender"/> implementations.
/// </remarks>
public class EmailData
{
    /// <summary>
    /// Gets or sets the list of primary recipients (To).
    /// </summary>
    /// <remarks>
    /// At least one recipient is typically required before sending.
    /// </remarks>
    public IList<Address> ToAddresses { get; set; } = new List<Address>();

    /// <summary>
    /// Gets or sets the list of carbon copy recipients (CC).
    /// </summary>
    /// <remarks>
    /// Visible to all recipients included in the message.
    /// </remarks>
    public IList<Address> CcAddresses { get; set; } = new List<Address>();

    /// <summary>
    /// Gets or sets the list of blind carbon copy recipients (BCC).
    /// </summary>
    /// <remarks>
    /// Hidden from other recipients; support depends on the sender implementation.
    /// </remarks>
    public IList<Address> BccAddresses { get; set; } = new List<Address>();

    /// <summary>
    /// Gets or sets the list of reply-to addresses.
    /// </summary>
    /// <remarks>
    /// Overrides the default reply target when recipients respond to the message.
    /// </remarks>
    public IList<Address> ReplyToAddresses { get; set; } = new List<Address>();

    /// <summary>
    /// Gets or sets the list of email attachments.
    /// </summary>
    /// <remarks>
    /// Duplicate attachment instances are ignored by the fluent API when adding items.
    /// </remarks>
    public IList<Attachment> Attachments { get; set; } = new List<Attachment>();

    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    /// <remarks>
    /// Required by most senders; may be preconfigured via dependency injection.
    /// </remarks>
    public Address? FromAddress { get; set; }

    /// <summary>
    /// Gets or sets the email subject.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the main email body.
    /// </summary>
    /// <remarks>
    /// Interpretation as HTML or plain text is controlled by <see cref="IsHtml"/>.
    /// </remarks>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the plain-text alternative body (multipart/alternative).
    /// </summary>
    /// <remarks>
    /// When set, senders such as SMTP may emit a multipart message with HTML and plain-text parts.
    /// </remarks>
    public string? PlaintextAlternativeBody { get; set; }

    /// <summary>
    /// Gets or sets the delivery priority.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Priority.Normal"/> when not explicitly changed.
    /// </remarks>
    public Priority Priority { get; set; } = Priority.Normal;

    /// <summary>
    /// Gets or sets tags associated with the email.
    /// </summary>
    /// <remarks>
    /// Provider-specific metadata; not all senders persist or transmit tags.
    /// </remarks>
    public IList<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether the main body is HTML.
    /// </summary>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets custom email headers.
    /// </summary>
    /// <remarks>
    /// Keys replace existing entries when the same header is added more than once.
    /// </remarks>
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
