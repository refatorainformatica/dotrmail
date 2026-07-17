namespace DotRMail.Core.Models;

/// <summary>
/// Represents an email address with an optional display name.
/// </summary>
/// <remarks>
/// Used throughout DotRMail for senders, recipients, and reply-to addresses.
/// Equality compares both <see cref="EmailAddress"/> and <see cref="Name"/>.
/// </remarks>
public class Address
{
    /// <summary>
    /// Gets or sets the recipient or sender display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    /// <remarks>
    /// Required for valid mail composition; defaults to an empty string when unset.
    /// </remarks>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of <see cref="Address"/>.
    /// </summary>
    /// <remarks>
    /// Creates an address with default empty values for use with object initializers.
    /// </remarks>
    public Address() { }

    /// <summary>
    /// Initializes a new instance of <see cref="Address"/> with address and name.
    /// </summary>
    /// <param name="emailAddress">Email address.</param>
    /// <param name="name">Optional display name.</param>
    /// <remarks>
    /// Preferred constructor when both values are known at creation time.
    /// </remarks>
    public Address(string emailAddress, string? name = null)
    {
        EmailAddress = emailAddress;
        Name = name;
    }

    /// <summary>
    /// Returns a formatted string representation of the address.
    /// </summary>
    /// <returns>
    /// The email address alone when <see cref="Name"/> is empty; otherwise
    /// <c>Name &lt;EmailAddress&gt;</c>.
    /// </returns>
    /// <remarks>
    /// Matches common RFC-style display formatting used in email headers.
    /// </remarks>
    public override string ToString() =>
        string.IsNullOrEmpty(Name) ? EmailAddress : $"{Name} <{EmailAddress}>";

    /// <summary>
    /// Determines whether the specified object is equal to the current address.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns><c>true</c> when both email address and name match; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Only compares against other <see cref="Address"/> instances.
    /// </remarks>
    public override bool Equals(object? obj)
    {
        if (obj is not Address other)
        {
            return false;
        }

        return EmailAddress == other.EmailAddress && Name == other.Name;
    }

    /// <summary>
    /// Returns a hash code for the current address.
    /// </summary>
    /// <returns>A hash code based on <see cref="EmailAddress"/> and <see cref="Name"/>.</returns>
    /// <remarks>
    /// Consistent with <see cref="Equals(object?)"/> for use in collections.
    /// </remarks>
    public override int GetHashCode() => HashCode.Combine(EmailAddress, Name);
}
