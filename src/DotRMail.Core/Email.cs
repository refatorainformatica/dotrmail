using System.Globalization;
using System.Reflection;
using DotRMail.Core.Abstractions;
using DotRMail.Core.Defaults;
using DotRMail.Core.Models;

namespace DotRMail.Core;

/// <summary>
/// Main implementation of the fluent API for composing and sending emails.
/// </summary>
/// <remarks>
/// Provides the default <see cref="IDotRMail"/> implementation with static defaults for
/// <see cref="DefaultRenderer"/> and <see cref="DefaultSender"/> used outside dependency injection.
/// </remarks>
public class Email : IDotRMail
{
    /// <summary>
    /// Gets or sets the default renderer used when none is configured.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="ReplaceRenderer"/>; replace globally for simple token substitution.
    /// </remarks>
    public static ITemplateRenderer DefaultRenderer { get; set; } = new ReplaceRenderer();

    /// <summary>
    /// Gets or sets the default sender used when none is configured.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="SaveToDiskSender"/> writing to <c>/tmp/dotrmail</c>.
    /// </remarks>
    public static ISender DefaultSender { get; set; } = new SaveToDiskSender("/tmp/dotrmail");

    /// <summary>
    /// Gets or sets the email data being composed.
    /// </summary>
    /// <remarks>
    /// Mutable container populated by fluent methods; read by senders at dispatch time.
    /// </remarks>
    public EmailData Data { get; set; }

    /// <summary>
    /// Gets or sets the template renderer in use.
    /// </summary>
    /// <remarks>
    /// Used when applying template-based body methods on this instance.
    /// </remarks>
    public ITemplateRenderer Renderer { get; set; }

    /// <summary>
    /// Gets or sets the email sender in use.
    /// </summary>
    /// <remarks>
    /// Invoked by <see cref="Send"/> and <see cref="SendAsync"/> to deliver the message.
    /// </remarks>
    public ISender Sender { get; set; }

    /// <summary>
    /// Creates a new instance with default renderer and sender.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="DefaultRenderer"/> and <see cref="DefaultSender"/> without a preset from address.
    /// </remarks>
    public Email()
        : this(DefaultRenderer, DefaultSender) { }

    /// <summary>
    /// Creates a new instance with custom renderer and sender.
    /// </summary>
    /// <param name="renderer">Template renderer for this instance.</param>
    /// <param name="sender">Sender for this instance.</param>
    /// <remarks>
    /// Does not configure a default from address.
    /// </remarks>
    public Email(ITemplateRenderer renderer, ISender sender)
        : this(renderer, sender, null, null) { }

    /// <summary>
    /// Creates a new instance with a default sender address.
    /// </summary>
    /// <param name="emailAddress">Default from email address.</param>
    /// <param name="name">Default from display name.</param>
    /// <remarks>
    /// Uses static defaults for renderer and sender dependencies.
    /// </remarks>
    public Email(string emailAddress, string name = "")
        : this(DefaultRenderer, DefaultSender, emailAddress, name) { }

    /// <summary>
    /// Creates a new instance with all dependencies and sender configured.
    /// </summary>
    /// <param name="renderer">Template renderer for this instance.</param>
    /// <param name="sender">Sender for this instance.</param>
    /// <param name="emailAddress">Optional default from email address.</param>
    /// <param name="name">Optional default from display name.</param>
    /// <remarks>
    /// Primary constructor used by dependency injection registration.
    /// </remarks>
    public Email(
        ITemplateRenderer renderer,
        ISender sender,
        string? emailAddress,
        string? name = ""
    )
    {
        Data = new EmailData
        {
            FromAddress = emailAddress is null ? null : new Address(emailAddress, name),
        };
        Renderer = renderer;
        Sender = sender;
    }

    /// <summary>
    /// Creates a new fluent instance with the sender address defined.
    /// </summary>
    /// <param name="emailAddress">Sender email address.</param>
    /// <param name="name">Optional sender display name.</param>
    /// <returns>A new configured <see cref="IDotRMail"/> instance.</returns>
    /// <remarks>
    /// Static factory entry point equivalent to calling <see cref="SetFrom"/> on a new instance.
    /// </remarks>
    public static IDotRMail From(string emailAddress, string? name = null) =>
        new Email { Data = { FromAddress = new Address(emailAddress, name ?? string.Empty) } };

    /// <summary>
    /// Sets the email sender address.
    /// </summary>
    /// <param name="emailAddress">Sender email address.</param>
    /// <param name="name">Optional sender display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Updates <see cref="EmailData.FromAddress"/> on the composed message.
    /// </remarks>
    public IDotRMail SetFrom(string emailAddress, string? name = null)
    {
        Data.FromAddress = new Address(emailAddress, name ?? string.Empty);
        return this;
    }

    /// <summary>
    /// Adds a primary recipient.
    /// </summary>
    /// <param name="emailAddress">Recipient email address.</param>
    /// <param name="name">Optional recipient display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// When <paramref name="emailAddress"/> contains semicolons, both address and name are split on <c>;</c>.
    /// </remarks>
    public IDotRMail To(string emailAddress, string? name = null)
    {
        if (emailAddress.Contains(';', StringComparison.Ordinal))
        {
            var nameSplit = name?.Split(';') ?? Array.Empty<string>();
            var addressSplit = emailAddress.Split(';');

            for (var i = 0; i < addressSplit.Length; i++)
            {
                var currentName = i < nameSplit.Length ? nameSplit[i] : string.Empty;
                Data.ToAddresses.Add(new Address(addressSplit[i].Trim(), currentName.Trim()));
            }
        }
        else
        {
            Data.ToAddresses.Add(new Address(emailAddress.Trim(), name?.Trim()));
        }

        return this;
    }

    /// <summary>
    /// Adds one or more primary recipients from a delimited string.
    /// </summary>
    /// <param name="emailAddress">One address or multiple addresses separated by <c>;</c>.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Each segment is trimmed before being appended to <see cref="EmailData.ToAddresses"/>.
    /// </remarks>
    public IDotRMail To(string emailAddress)
    {
        if (emailAddress.Contains(';', StringComparison.Ordinal))
        {
            foreach (var address in emailAddress.Split(';'))
            {
                Data.ToAddresses.Add(new Address(address.Trim()));
            }
        }
        else
        {
            Data.ToAddresses.Add(new Address(emailAddress.Trim()));
        }

        return this;
    }

    /// <summary>
    /// Adds multiple primary recipients.
    /// </summary>
    /// <param name="mailAddresses">Recipients to append.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends all items to <see cref="EmailData.ToAddresses"/> without removing existing entries.
    /// </remarks>
    public IDotRMail To(IEnumerable<Address> mailAddresses)
    {
        Data.ToAddresses.AddRange(mailAddresses);
        return this;
    }

    /// <summary>
    /// Adds a carbon copy (CC) recipient.
    /// </summary>
    /// <param name="emailAddress">CC email address.</param>
    /// <param name="name">Optional display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// CC recipients are visible to all message recipients.
    /// </remarks>
    public IDotRMail CC(string emailAddress, string name = "")
    {
        Data.CcAddresses.Add(new Address(emailAddress, name));
        return this;
    }

    /// <summary>
    /// Adds multiple carbon copy (CC) recipients.
    /// </summary>
    /// <param name="mailAddresses">CC recipients to append.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends all items to <see cref="EmailData.CcAddresses"/>.
    /// </remarks>
    public IDotRMail CC(IEnumerable<Address> mailAddresses)
    {
        Data.CcAddresses.AddRange(mailAddresses);
        return this;
    }

    /// <summary>
    /// Adds a blind carbon copy (BCC) recipient.
    /// </summary>
    /// <param name="emailAddress">BCC email address.</param>
    /// <param name="name">Optional display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// BCC recipients are hidden from other recipients; sender support may vary.
    /// </remarks>
    public IDotRMail BCC(string emailAddress, string name = "")
    {
        Data.BccAddresses.Add(new Address(emailAddress, name));
        return this;
    }

    /// <summary>
    /// Adds multiple blind carbon copy (BCC) recipients.
    /// </summary>
    /// <param name="mailAddresses">BCC recipients to append.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends all items to <see cref="EmailData.BccAddresses"/>.
    /// </remarks>
    public IDotRMail BCC(IEnumerable<Address> mailAddresses)
    {
        Data.BccAddresses.AddRange(mailAddresses);
        return this;
    }

    /// <summary>
    /// Adds a reply-to address without a display name.
    /// </summary>
    /// <param name="address">Reply-to email address.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends to <see cref="EmailData.ReplyToAddresses"/>.
    /// </remarks>
    public IDotRMail ReplyTo(string address)
    {
        Data.ReplyToAddresses.Add(new Address(address));
        return this;
    }

    /// <summary>
    /// Adds a reply-to address with a display name.
    /// </summary>
    /// <param name="address">Reply-to email address.</param>
    /// <param name="name">Reply-to display name.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Appends to <see cref="EmailData.ReplyToAddresses"/>.
    /// </remarks>
    public IDotRMail ReplyTo(string address, string name)
    {
        Data.ReplyToAddresses.Add(new Address(address, name));
        return this;
    }

    /// <summary>
    /// Sets the email subject.
    /// </summary>
    /// <param name="subject">Message subject line.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Assigns <see cref="EmailData.Subject"/>.
    /// </remarks>
    public IDotRMail Subject(string subject)
    {
        Data.Subject = subject;
        return this;
    }

    /// <summary>
    /// Sets the email body.
    /// </summary>
    /// <param name="body">Message body content.</param>
    /// <param name="isHtml">When <c>true</c>, marks the body as HTML.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Updates both <see cref="EmailData.Body"/> and <see cref="EmailData.IsHtml"/>.
    /// </remarks>
    public IDotRMail Body(string body, bool isHtml = false)
    {
        Data.IsHtml = isHtml;
        Data.Body = body;
        return this;
    }

    /// <summary>
    /// Sets the plain-text alternative body.
    /// </summary>
    /// <param name="body">Plain-text alternative content.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Used for multipart messages when an HTML body is also present.
    /// </remarks>
    public IDotRMail PlaintextAlternativeBody(string body)
    {
        Data.PlaintextAlternativeBody = body;
        return this;
    }

    /// <summary>
    /// Sets high delivery priority.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Assigns <see cref="Priority.High"/> to <see cref="EmailData.Priority"/>.
    /// </remarks>
    public IDotRMail HighPriority()
    {
        Data.Priority = Priority.High;
        return this;
    }

    /// <summary>
    /// Sets low delivery priority.
    /// </summary>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Assigns <see cref="Priority.Low"/> to <see cref="EmailData.Priority"/>.
    /// </remarks>
    public IDotRMail LowPriority()
    {
        Data.Priority = Priority.Low;
        return this;
    }

    /// <summary>
    /// Replaces the template renderer for this instance.
    /// </summary>
    /// <param name="renderer">Renderer to use for subsequent template operations.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Affects only this builder instance unless the same renderer is registered globally.
    /// </remarks>
    public IDotRMail UsingTemplateEngine(ITemplateRenderer renderer)
    {
        Renderer = renderer;
        return this;
    }

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
    public IDotRMail UsingTemplateFromEmbedded<T>(
        string path,
        T model,
        Assembly assembly,
        bool isHtml = true
    )
    {
        var template = EmbeddedResourceHelper.GetResourceAsString(assembly, path);
        return ApplyTemplate(template, model, isHtml);
    }

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
    public IDotRMail UsingTemplateFromFile<T>(string filename, T model, bool isHtml = true)
    {
        var template = File.ReadAllText(filename);
        return ApplyTemplate(template, model, isHtml);
    }

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
    public IDotRMail UsingCultureTemplateFromFile<T>(
        string filename,
        T model,
        CultureInfo culture,
        bool isHtml = true
    )
    {
        var cultureFile = GetCultureFileName(filename, culture);
        return UsingTemplateFromFile(cultureFile, model, isHtml);
    }

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
    public IDotRMail UsingTemplate<T>(string template, T model, bool isHtml = true) =>
        ApplyTemplate(template, model, isHtml);

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
    public IDotRMail PlaintextAlternativeUsingTemplateFromEmbedded<T>(
        string path,
        T model,
        Assembly assembly
    )
    {
        var template = EmbeddedResourceHelper.GetResourceAsString(assembly, path);
        return ApplyPlaintextAlternativeTemplate(template, model);
    }

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
    public IDotRMail PlaintextAlternativeUsingTemplateFromFile<T>(string filename, T model)
    {
        var template = File.ReadAllText(filename);
        return ApplyPlaintextAlternativeTemplate(template, model);
    }

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
    public IDotRMail PlaintextAlternativeUsingCultureTemplateFromFile<T>(
        string filename,
        T model,
        CultureInfo culture
    )
    {
        var cultureFile = GetCultureFileName(filename, culture);
        return PlaintextAlternativeUsingTemplateFromFile(cultureFile, model);
    }

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
    public IDotRMail PlaintextAlternativeUsingTemplate<T>(string template, T model) =>
        ApplyPlaintextAlternativeTemplate(template, model);

    /// <summary>
    /// Adds an attachment to the email.
    /// </summary>
    /// <param name="attachment">Attachment to add.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Ignores duplicate attachment references already present in <see cref="EmailData.Attachments"/>.
    /// </remarks>
    public IDotRMail Attach(Attachment attachment)
    {
        if (!Data.Attachments.Contains(attachment))
        {
            Data.Attachments.Add(attachment);
        }

        return this;
    }

    /// <summary>
    /// Adds multiple attachments to the email.
    /// </summary>
    /// <param name="attachments">Attachments to add.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Skips attachments already contained in <see cref="EmailData.Attachments"/>.
    /// </remarks>
    public IDotRMail Attach(IEnumerable<Attachment> attachments)
    {
        foreach (var attachment in attachments.Where(a => !Data.Attachments.Contains(a)))
        {
            Data.Attachments.Add(attachment);
        }

        return this;
    }

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
    public IDotRMail AttachFromFilename(
        string filename,
        string? contentType = null,
        string? attachmentName = null
    )
    {
        var stream = File.OpenRead(filename);
        Attach(
            new Attachment
            {
                Data = stream,
                Filename = attachmentName ?? Path.GetFileName(filename),
                ContentType = contentType,
            }
        );

        return this;
    }

    /// <summary>
    /// Adds a tag to the email.
    /// </summary>
    /// <param name="tag">Tag value to append.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Tags are provider-specific metadata stored in <see cref="EmailData.Tags"/>.
    /// </remarks>
    public IDotRMail Tag(string tag)
    {
        Data.Tags.Add(tag);
        return this;
    }

    /// <summary>
    /// Adds a custom header to the email.
    /// </summary>
    /// <param name="header">Header name.</param>
    /// <param name="value">Header value.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Replaces an existing header with the same name in <see cref="EmailData.Headers"/>.
    /// </remarks>
    public IDotRMail Header(string header, string value)
    {
        Data.Headers[header] = value;
        return this;
    }

    /// <summary>
    /// Sends the email synchronously.
    /// </summary>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Delegates to <see cref="Sender"/> using the current composed state.
    /// </remarks>
    public virtual SendResponse Send(CancellationToken? token = null) => Sender.Send(this, token);

    /// <summary>
    /// Sends the email asynchronously.
    /// </summary>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>Send operation result.</returns>
    /// <remarks>
    /// Preferred dispatch method for async application flows.
    /// </remarks>
    public virtual Task<SendResponse> SendAsync(CancellationToken? token = null) =>
        Sender.SendAsync(this, token);

    /// <summary>
    /// Renders a template and assigns the result to the main body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="template">Template content.</param>
    /// <param name="model">Template model.</param>
    /// <param name="isHtml">When <c>true</c>, marks the rendered body as HTML.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Shared helper used by template-based body methods.
    /// </remarks>
    private IDotRMail ApplyTemplate<T>(string template, T model, bool isHtml)
    {
        var result = Renderer.Parse(template, model, isHtml);
        Data.IsHtml = isHtml;
        Data.Body = result;
        return this;
    }

    /// <summary>
    /// Renders a template and assigns the result to the plain-text alternative body.
    /// </summary>
    /// <typeparam name="T">Model type passed to the renderer.</typeparam>
    /// <param name="template">Template content.</param>
    /// <param name="model">Template model.</param>
    /// <returns>The current instance for chaining.</returns>
    /// <remarks>
    /// Forces non-HTML rendering via <see cref="ITemplateRenderer.Parse{T}"/>.
    /// </remarks>
    private IDotRMail ApplyPlaintextAlternativeTemplate<T>(string template, T model)
    {
        Data.PlaintextAlternativeBody = Renderer.Parse(template, model, false);
        return this;
    }

    /// <summary>
    /// Resolves a culture-specific template file name.
    /// </summary>
    /// <param name="fileName">Base template file path.</param>
    /// <param name="culture">Culture used to build the localized extension.</param>
    /// <returns>The localized file path when it exists; otherwise <paramref name="fileName"/>.</returns>
    /// <remarks>
    /// Inserts the culture name before the file extension, for example <c>template.pt-BR.html</c>.
    /// </remarks>
    private static string GetCultureFileName(string fileName, CultureInfo culture)
    {
        var extension = Path.GetExtension(fileName);
        var cultureExtension = $"{culture.Name}{extension}";
        var cultureFile = Path.ChangeExtension(fileName, cultureExtension);
        return File.Exists(cultureFile) ? cultureFile : fileName;
    }
}
