namespace DotRMail.Core.Models;

/// <summary>
/// Represents an email attachment.
/// </summary>
/// <remarks>
/// Attachments are added to <see cref="EmailData.Attachments"/> and consumed by sender implementations.
/// </remarks>
public class Attachment
{
    /// <summary>
    /// Indicates whether the attachment is an inline image (CID).
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, <see cref="ContentId"/> should identify the inline resource in HTML bodies.
    /// </remarks>
    public bool IsInline { get; set; }

    /// <summary>
    /// Gets or sets the attachment file name.
    /// </summary>
    /// <remarks>
    /// Used as the filename in MIME parts when the email is sent.
    /// </remarks>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stream containing the attachment content.
    /// </summary>
    /// <remarks>
    /// Must be readable when the sender processes the attachment; callers own stream disposal.
    /// </remarks>
    public Stream? Data { get; set; }

    /// <summary>
    /// Gets or sets the MIME content type.
    /// </summary>
    /// <remarks>
    /// Optional; senders may infer a type when this value is null.
    /// </remarks>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the content identifier for inline attachments.
    /// </summary>
    /// <remarks>
    /// Referenced from HTML via <c>cid:</c> URLs when <see cref="IsInline"/> is <c>true</c>.
    /// </remarks>
    public string? ContentId { get; set; }
}
