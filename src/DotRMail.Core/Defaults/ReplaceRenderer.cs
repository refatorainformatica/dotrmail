using System.Reflection;
using DotRMail.Core.Abstractions;

namespace DotRMail.Core.Defaults;

/// <summary>
/// Simple renderer that replaces placeholders in the format <c>##Property##</c>.
/// </summary>
/// <remarks>
/// Default template engine for DotRMail; uses reflection over public properties on the model type.
/// </remarks>
public class ReplaceRenderer : ITemplateRenderer
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
    public string Parse<T>(string template, T model, bool isHtml = true)
    {
        if (model is null)
        {
            return template;
        }

        foreach (var property in model.GetType().GetRuntimeProperties())
        {
            var value = property.GetValue(model)?.ToString() ?? string.Empty;
            template = template.Replace($"##{property.Name}##", value, StringComparison.Ordinal);
        }

        return template;
    }

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
    public Task<string> ParseAsync<T>(string template, T model, bool isHtml = true) =>
        Task.FromResult(Parse(template, model, isHtml));
}
