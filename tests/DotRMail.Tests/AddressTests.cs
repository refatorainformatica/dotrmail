using DotRMail.Core.Models;

namespace DotRMail.Tests;

/// <summary>
/// Tests for <see cref="Address"/> formatting and equality behavior.
/// </summary>
/// <remarks>
/// Covers string representation and value comparison used throughout recipient handling.
/// </remarks>
public class AddressTests
{
    /// <summary>
    /// Verifies that an address without a display name formats as the email alone.
    /// </summary>
    /// <remarks>
    /// Ensures <see cref="Address.ToString"/> omits angle brackets when <see cref="Address.Name"/> is empty.
    /// </remarks>
    [Fact]
    public void ToString_WithoutName_ShouldReturnEmailOnly()
    {
        var address = new Address("user@example.com");
        Assert.Equal("user@example.com", address.ToString());
    }

    /// <summary>
    /// Verifies that an address with a display name uses RFC-style formatting.
    /// </summary>
    /// <remarks>
    /// Expected format is <c>Name &lt;email&gt;</c>.
    /// </remarks>
    [Fact]
    public void ToString_WithName_ShouldReturnFormattedAddress()
    {
        var address = new Address("user@example.com", "User");
        Assert.Equal("User <user@example.com>", address.ToString());
    }

    /// <summary>
    /// Verifies that equality compares both email address and display name.
    /// </summary>
    /// <remarks>
    /// Different email addresses with the same name must not be considered equal.
    /// </remarks>
    [Fact]
    public void Equals_ShouldCompareEmailAndName()
    {
        var first = new Address("user@example.com", "User");
        var second = new Address("user@example.com", "User");
        var third = new Address("other@example.com", "User");

        Assert.True(first.Equals(second));
        Assert.False(first.Equals(third));
    }
}
