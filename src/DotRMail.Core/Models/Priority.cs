namespace DotRMail.Core.Models;

/// <summary>
/// Defines the delivery priority of an email.
/// </summary>
/// <remarks>
/// Mapped to provider-specific priority values by senders such as SMTP.
/// </remarks>
public enum Priority
{
    /// <summary>
    /// High delivery priority.
    /// </summary>
    /// <remarks>
    /// Indicates time-sensitive content that should be delivered promptly.
    /// </remarks>
    High = 1,

    /// <summary>
    /// Normal delivery priority.
    /// </summary>
    /// <remarks>
    /// Default priority used when none is explicitly set on the message.
    /// </remarks>
    Normal = 2,

    /// <summary>
    /// Low delivery priority.
    /// </summary>
    /// <remarks>
    /// Suitable for non-urgent or bulk notifications.
    /// </remarks>
    Low = 3,
}
