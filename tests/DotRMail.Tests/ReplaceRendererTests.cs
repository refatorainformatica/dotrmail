using DotRMail.Core.Defaults;

namespace DotRMail.Tests;

/// <summary>
/// Tests for <see cref="ReplaceRenderer"/> placeholder substitution.
/// </summary>
/// <remarks>
/// Validates the default token replacement engine used by DotRMail.
/// </remarks>
public class ReplaceRendererTests
{
    /// <summary>
    /// Verifies that property placeholders are replaced with model values.
    /// </summary>
    /// <remarks>
    /// Placeholders use the <c>##PropertyName##</c> format.
    /// </remarks>
    [Fact]
    public void Parse_ShouldReplacePropertyPlaceholders()
    {
        var renderer = new ReplaceRenderer();
        var result = renderer.Parse(
            "Hello ##Name##, your balance is ##Balance##",
            new { Name = "John", Balance = 100 }
        );

        Assert.Equal("Hello John, your balance is 100", result);
    }

    /// <summary>
    /// Verifies that ParseAsync returns the same output as the synchronous Parse method.
    /// </summary>
    /// <remarks>
    /// Async path wraps synchronous rendering for interface compatibility.
    /// </remarks>
    [Fact]
    public async Task ParseAsync_ShouldReturnSameResultAsParse()
    {
        var renderer = new ReplaceRenderer();
        var sync = renderer.Parse("##Title##", new { Title = "DotRMail" });
        var async = await renderer.ParseAsync("##Title##", new { Title = "DotRMail" });

        Assert.Equal(sync, async);
    }
}
