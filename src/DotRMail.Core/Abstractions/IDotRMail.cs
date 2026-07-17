using System.Globalization;
using System.Reflection;
using DotRMail.Core.Models;

namespace DotRMail.Core.Abstractions;

/// <summary>
/// Contract for the fluent API used to compose and send emails.
/// </summary>
/// <remarks>
/// Methods return <see cref="IDotRMail"/> to enable chaining. State is stored in <see cref="Data"/>
/// until <see cref="Send"/> or <see cref="SendAsync"/> delegates to <see cref="Sender"/>.
/// </remarks>
public interface IDotRMail : IHideObjectMembers
{
    /// <summary>
    /// Gets or sets the email data being composed.
    /// </summary>
    /// <remarks>
    /// Mutable container populated by fluent methods; read by senders at dispatch time.
    /// </remarks>
    EmailData Data { get; set; }

    /// <summary>
    /// Gets or sets the template renderer in use.
    /// </summary>
    /// <remarks>
    /// Used when applying template-based body methods on this instance.
    /// </remarks>
    ITemplateRenderer Renderer { get; set; }

    /// <summary>
    /// Gets or sets the email sender in use.
    /// </summary>
    /// <remarks>
    /// Invoked by <see cref="Send"/> and <see cref="SendAsync"/> to deliver the message.
    /// </remarks>
    ISender Sender { get; set; }

    /// <summary>
    /// Sets the email sender address.
    /// </summary>
    /// <param name="emailAddress">Sender email address.</param>
    /// <param name="name">Optional sender display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Updates <see cref="EmailData.FromAddress"/> on the composed message.
    /// </remarks>
    IDotRMail SetFrom(string emailAddress, string? name = null);

    /// <summary>
    /// Adds a primary recipient.
    /// </summary>
    /// <param name="emailAddress">Recipient email address.</param>
    /// <param name="name">Optional recipient display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// When <paramref name="emailAddress"/> contains semicolons, both address and name are split on <c>;</c>.
    /// </remarks>
    IDotRMail To(string emailAddress, string? name = null);

    /// <summary>
    /// Adds one or more primary recipients from a delimited string.
    /// </summary>
    /// <param name="emailAddress">One address or multiple addresses separated by <c>;</c>.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Each segment is trimmed before being appended to <see cref="EmailData.ToAddresses"/>.
    /// </remarks>
    IDotRMail To(string emailAddress);

    /// <summary>
    /// Adds multiple primary recipients.
    /// </summary>
    /// <param name="mailAddresses">Recipients to append.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends all items to <see cref="EmailData.ToAddresses"/> without removing existing entries.
    /// </remarks>
    IDotRMail To(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Adds a carbon copy (CC) recipient.
    /// </summary>
    /// <param name="emailAddress">CC email address.</param>
    /// <param name="name">Optional display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// CC recipients are visible to all message recipients.
    /// </remarks>
    IDotRMail CC(string emailAddress, string name = "");

    /// <summary>
    /// Adds multiple carbon copy (CC) recipients.
    /// </summary>
    /// <param name="mailAddresses">CC recipients to append.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends all items to <see cref="EmailData.CcAddresses"/>.
    /// </remarks>
    IDotRMail CC(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Adds a blind carbon copy (BCC) recipient.
    /// </summary>
    /// <param name="emailAddress">BCC email address.</param>
    /// <param name="name">Optional display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// BCC recipients are hidden from other recipients; sender support may vary.
    /// </remarks>
    IDotRMail BCC(string emailAddress, string name = "");

    /// <summary>
    /// Adds multiple blind carbon copy (BCC) recipients.
    /// </summary>
    /// <param name="mailAddresses">BCC recipients to append.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends all items to <see cref="EmailData.BccAddresses"/>.
    /// </remarks>
    IDotRMail BCC(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Adds a reply-to address without a display name.
    /// </summary>
    /// <param name="address">Reply-to email address.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends to <see cref="EmailData.ReplyToAddresses"/>.
    /// </remarks>
    IDotRMail ReplyTo(string address);

    /// <summary>
    /// Adds a reply-to address with a display name.
    /// </summary>
    /// <param name="address">Reply-to email address.</param>
    /// <param name="name">Reply-to display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends to <see cref="EmailData.ReplyToAddresses"/>.
    /// </remarks>
    IDotRMail ReplyTo(string address, string name);

    /// <summary>
    /// Sets the email subject.
    /// </summary>
    /// <param name="subject">Message subject line.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Assigns <see cref="EmailData.Subject"/>.
    /// </remarks>
    IDotRMail Subject(string subject);

    /// <summary>
    /// Sets the email body.
    /// </summary>
    /// <param name="body">Message body content.</param>
    /// <param name="isHtml">When <c>true</c>, marks the body as HTML.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Updates both <see cref="EmailData.Body"/> and <see cref="EmailData.IsHtml"/>.
    /// </remarks>
    IDotRMail Body(string body, bool isHtml = false);

    /// <summary>
    /// Sets the plain-text alternative body.
    /// </summary>
    /// <param name="body">Plain-text alternative content.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Used for multipart messages when an HTML body is also present.
    /// </remarks>
    IDotRMail PlaintextAlternativeBody(string body);

    /// <summary>
    /// Sets high delivery priority.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Assigns <see cref="Priority.High"/> to <see cref="EmailData.Priority"/>.
    /// </remarks>
    IDotRMail HighPriority();

    /// <summary>
    /// Sets low delivery priority.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Assigns <see cref="Priority.Low"/> to <see cref="EmailData.Priority"/>.
    /// </remarks>
    IDotRMail LowPriority();

    /// <summary>
    /// Replaces the template renderer for this instance.
    /// </summary>
    /// <param name="renderer">Renderer to use for subsequent template operations.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Affects only this builder instance unless the same renderer is registered globally.
    /// </remarks>
    IDotRMail UsingTemplateEngine(ITemplateRenderer renderer);

    /// <summary>
    /// Renders an embedded template as the email body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="path">Embedded resource name.</param>
    /// <param name="model">Template model.</param>
    /// <param name="assembly">Assembly containing the embedded resource.</param>
    /// <param name="isHtml">When <c>true</c>, treats rendered output as HTML.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Loads the resource from <paramref name="assembly"/> before rendering with <see cref="Renderer"/>.
    /// </remarks>
    IDotRMail UsingTemplateFromEmbedded<T>(string path, T model, Assembly assembly, bool isHtml = true);

    /// <summary>
    /// Renders a template file as the email body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="filename">Path to the template file.</param>
    /// <param name="model">Template model.</param>
    /// <param name="isHtml">When <c>true</c>, treats rendered output as HTML.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Reads the entire file synchronously before rendering.
    /// </remarks>
    IDotRMail UsingTemplateFromFile<T>(string filename, T model, bool isHtml = true);

    /// <summary>
    /// Renders a culture-specific template file as the email body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="filename">Base template file path.</param>
    /// <param name="model">Template model.</param>
    /// <param name="culture">Culture used to resolve a localized file name.</param>
    /// <param name="isHtml">When <c>true</c>, treats rendered output as HTML.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Falls back to <paramref name="filename"/> when no culture-specific file exists.
    /// </remarks>
    IDotRMail UsingCultureTemplateFromFile<T>(string filename, T model, CultureInfo culture, bool isHtml = true);

    /// <summary>
    /// Renders an inline template as the email body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="template">Template content.</param>
    /// <param name="model">Template model.</param>
    /// <param name="isHtml">When <c>true</c>, treats rendered output as HTML.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Does not read from disk or embedded resources.
    /// </remarks>
    IDotRMail UsingTemplate<T>(string template, T model, bool isHtml = true);

    /// <summary>
    /// Renders an embedded template as the plain-text alternative body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="path">Embedded resource name.</param>
    /// <param name="model">Template model.</param>
    /// <param name="assembly">Assembly containing the embedded resource.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Rendered output is stored in <see cref="EmailData.PlaintextAlternativeBody"/>.
    /// </remarks>
    IDotRMail PlaintextAlternativeUsingTemplateFromEmbedded<T>(string path, T model, Assembly assembly);

    /// <summary>
    /// Renders a template file as the plain-text alternative body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="filename">Path to the template file.</param>
    /// <param name="model">Template model.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Rendered output is stored in <see cref="EmailData.PlaintextAlternativeBody"/>.
    /// </remarks>
    IDotRMail PlaintextAlternativeUsingTemplateFromFile<T>(string filename, T model);

    /// <summary>
    /// Renders a culture-specific template file as the plain-text alternative body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="filename">Base template file path.</param>
    /// <param name="model">Template model.</param>
    /// <param name="culture">Culture used to resolve a localized file name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Falls back to <paramref name="filename"/> when no culture-specific file exists.
    /// </remarks>
    IDotRMail PlaintextAlternativeUsingCultureTemplateFromFile<T>(string filename, T model, CultureInfo culture);

    /// <summary>
    /// Renders an inline template as the plain-text alternative body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="template">Template content.</param>
    /// <param name="model">Template model.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Rendered output is stored in <see cref="EmailData.PlaintextAlternativeBody"/>.
    /// </remarks>
    IDotRMail PlaintextAlternativeUsingTemplate<T>(string template, T model);

    /// <summary>
    /// Adds an attachment to the email.
    /// </summary>
    /// <param name="attachment">Attachment to add.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Ignores duplicate attachment references already present in <see cref="EmailData.Attachments"/>.
    /// </remarks>
    IDotRMail Attach(Attachment attachment);

    /// <summary>
    /// Adds multiple attachments to the email.
    /// </summary>
    /// <param name="attachments">Attachments to add.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Skips attachments already contained in <see cref="EmailData.Attachments"/>.
    /// </remarks>
    IDotRMail Attach(IEnumerable<Attachment> attachments);

    /// <summary>
    /// Adds an attachment from a file on disk.
    /// </summary>
    /// <param name="filename">Path to the file to attach.</param>
    /// <param name="contentType">Optional MIME content type.</param>
    /// <param name="attachmentName">Optional attachment file name override.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Opens a read stream to the file; the caller is responsible for stream lifetime if reused.
    /// </remarks>
    IDotRMail AttachFromFilename(string filename, string? contentType = null, string? attachmentName = null);

    /// <summary>
    /// Adds a tag to the email.
    /// </summary>
    /// <param name="tag">Tag value to append.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Tags are provider-specific metadata stored in <see cref="EmailData.Tags"/>.
    /// </remarks>
    IDotRMail Tag(string tag);

    /// <summary>
    /// Adds a custom header to the email.
    /// </summary>
    /// <param name="header">Header name.</param>
    /// <param name="value">Header value.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Replaces an existing header with the same name in <see cref="EmailData.Headers"/>.
    /// </remarks>
    IDotRMail Header(string header, string value);

    /// <summary>
    /// Sends the email synchronously.
    /// </summary>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Delegates to <see cref="Sender"/> using the current composed state.
    /// </remarks>
    SendResponse Send(CancellationToken? token = null);

    /// <summary>
    /// Sends the email asynchronously.
    /// </summary>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Preferred dispatch method for async application flows.
    /// </remarks>
    Task<SendResponse> SendAsync(CancellationToken? token = null);
}
