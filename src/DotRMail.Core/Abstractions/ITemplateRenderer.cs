namespace DotRMail.Core.Abstractions;

/// <summary>
/// Contract for email template rendering.
/// </summary>
/// <remarks>
/// Renderers transform template strings and model objects into final email body content.
/// Replace the default <see cref="Defaults.ReplaceRenderer"/> with engines such as Liquid when needed.
/// </remarks>
public interface ITemplateRenderer
{
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
    string Parse<T>(string template, T model, bool isHtml = true);

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
    Task<string> ParseAsync<T>(string template, T model, bool isHtml = true);
}
