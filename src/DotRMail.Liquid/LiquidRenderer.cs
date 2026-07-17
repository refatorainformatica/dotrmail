using DotRMail.Core.Abstractions;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace DotRMail.Liquid;

/// <summary>
/// Configuration options for the Liquid renderer.
/// </summary>
/// <remarks>
/// Bound through dependency injection when registering <see cref="LiquidRenderer"/>.
/// </remarks>
public class LiquidRendererOptions
{
    /// <summary>
    /// Gets or sets the file provider for external templates.
    /// </summary>
    /// <remarks>
    /// Passed to Fluid template contexts for include and layout resolution.
    /// </remarks>
    public IFileProvider? FileProvider { get; set; }

    /// <summary>
    /// Gets or sets Fluid parser and rendering options.
    /// </summary>
    /// <remarks>
    /// Defaults to a new <see cref="TemplateOptions"/> instance when unset.
    /// </remarks>
    public TemplateOptions TemplateOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets a callback for additional template context configuration.
    /// </summary>
    /// <remarks>
    /// Invoked after the base context is created and before rendering begins.
    /// </remarks>
    public Action<TemplateContext, object?>? ConfigureTemplateContext { get; set; }
}

/// <summary>
/// Liquid template renderer using the Fluid library.
/// </summary>
/// <remarks>
/// Implements <see cref="ITemplateRenderer"/> with Shopify Liquid syntax support.
/// </remarks>
public class LiquidRenderer : ITemplateRenderer
{
    /// <summary>
    /// Resolved renderer options from dependency injection.
    /// </summary>
    /// <remarks>
    /// Read on each render to pick up configured file providers and template options.
    /// </remarks>
    private readonly IOptions<LiquidRendererOptions> _options;

    /// <summary>
    /// Fluid parser reused across render operations.
    /// </summary>
    /// <remarks>
    /// Thread-safe for parsing template strings into executable Fluid templates.
    /// </remarks>
    private readonly FluidParser _parser = new();

    /// <summary>
    /// Initializes the renderer with configured options.
    /// </summary>
    /// <param name="options">Renderer configuration supplied by dependency injection.</param>
    /// <remarks>
    /// Options are accessed through <see cref="IOptions{TOptions}.Value"/> at render time.
    /// </remarks>
    public LiquidRenderer(IOptions<LiquidRendererOptions> options)
    {
        _options = options;
    }

    /// <summary>
    /// Renders a template with the provided model.
    /// </summary>
    /// <typeparam name="T">Model data type.</typeparam>
    /// <param name="template">Template content.</param>
    /// <param name="model">Model used for substitution.</param>
    /// <param name="isHtml">Indicates whether the output should be treated as HTML.</param>
    /// <returns>Rendered content.</returns>
    /// <remarks>
    /// Synchronous wrapper; implementations may delegate to <see cref="ParseAsync{T}"/>.
    /// </remarks>
    public string Parse<T>(string template, T model, bool isHtml = true) =>
        ParseAsync(template, model, isHtml).GetAwaiter().GetResult();

    /// <summary>
    /// Renders a template asynchronously.
    /// </summary>
    /// <typeparam name="T">Model data type.</typeparam>
    /// <param name="template">Template content.</param>
    /// <param name="model">Model used for substitution.</param>
    /// <param name="isHtml">Indicates whether the output should be treated as HTML.</param>
    /// <returns>Rendered content.</returns>
    /// <remarks>
    /// Used by the fluent API when template rendering may involve I/O or async template engines.
    /// </remarks>
    public async Task<string> ParseAsync<T>(string template, T model, bool isHtml = true)
    {
        var rendererOptions = _options.Value;
        var fluidTemplate = ParseTemplate(template);

        var context = new TemplateContext(model, rendererOptions.TemplateOptions)
        {
            Options = { FileProvider = rendererOptions.FileProvider },
        };

        rendererOptions.ConfigureTemplateContext?.Invoke(context, model);

        return await fluidTemplate.RenderAsync(context, NullEncoder.Default);
    }

    /// <summary>
    /// Parses Liquid template content into a Fluid template.
    /// </summary>
    /// <param name="content">Template source text.</param>
    /// <returns>A parsed Fluid template ready for rendering.</returns>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> when parsing fails.
    /// </remarks>
    private IFluidTemplate ParseTemplate(string content)
    {
        if (!_parser.TryParse(content, out var template, out var errors))
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }

        return template;
    }
}
