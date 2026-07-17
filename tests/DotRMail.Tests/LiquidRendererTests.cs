using DotRMail.Liquid;
using Microsoft.Extensions.Options;

namespace DotRMail.Tests;

/// <summary>
/// Tests for <see cref="LiquidRenderer"/> template parsing and rendering.
/// </summary>
/// <remarks>
/// Covers successful Liquid rendering and invalid template error handling.
/// </remarks>
public class LiquidRendererTests
{
    /// <summary>
    /// Verifies that ParseAsync renders Liquid placeholders from the model.
    /// </summary>
    /// <remarks>
    /// Uses default <see cref="LiquidRendererOptions"/> without custom file providers.
    /// </remarks>
    [Fact]
    public async Task ParseAsync_ShouldRenderLiquidTemplate()
    {
        var renderer = new LiquidRenderer(Options.Create(new LiquidRendererOptions()));
        var result = await renderer.ParseAsync(
            "Hello {{ name }}, welcome to {{ product }}!",
            new { name = "Anna", product = "DotRMail" }
        );

        Assert.Equal("Hello Anna, welcome to DotRMail!", result);
    }

    /// <summary>
    /// Verifies that invalid Liquid syntax throws an InvalidOperationException.
    /// </summary>
    /// <remarks>
    /// Parser errors are aggregated into the exception message.
    /// </remarks>
    [Fact]
    public void Parse_WithInvalidTemplate_ShouldThrow()
    {
        var renderer = new LiquidRenderer(Options.Create(new LiquidRendererOptions()));

        Assert.Throws<InvalidOperationException>(() =>
            renderer.Parse("{% if %}", new { name = "Test" })
        );
    }
}
