using System.Reflection;

namespace DotRMail.Core;

/// <summary>
/// Loads embedded assembly resources as strings.
/// </summary>
/// <remarks>
/// Used by template methods that read content from embedded resources in consuming assemblies.
/// </remarks>
internal static class EmbeddedResourceHelper
{
    /// <summary>
    /// Reads an embedded resource from an assembly as a string.
    /// </summary>
    /// <param name="assembly">Assembly that contains the resource.</param>
    /// <param name="path">Manifest resource name.</param>
    /// <returns>The full text content of the resource.</returns>
    /// <remarks>
    /// Throws <see cref="InvalidOperationException"/> when the resource name is not found.
    /// </remarks>
    internal static string GetResourceAsString(Assembly assembly, string path)
    {
        using var stream =
            assembly.GetManifestResourceStream(path)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{path}' was not found in {assembly.FullName}."
            );
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
