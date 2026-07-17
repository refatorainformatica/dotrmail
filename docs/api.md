# API reference — DotRMail

Reference documentation for the main public types and methods.

## `IDotRMail`

Main fluent API interface. All configuration methods return `IDotRMail` for chaining.

### Sender and recipients

| Method | Description |
|--------|-------------|
| `SetFrom(email, name?)` | Sets the sender |
| `To(email, name?)` | Adds a recipient (supports `;` for multiple) |
| `To(addresses)` | Adds a list of recipients |
| `CC(email, name?)` | Adds a carbon copy recipient |
| `BCC(email, name?)` | Adds a blind carbon copy recipient |
| `ReplyTo(email, name?)` | Adds a reply-to address |

### Content

| Method | Description |
|--------|-------------|
| `Subject(text)` | Sets the subject |
| `Body(text, isHtml?)` | Sets the body (HTML or plain text) |
| `PlaintextAlternativeBody(text)` | Sets the plain-text alternative body |
| `HighPriority()` | Sets high priority |
| `LowPriority()` | Sets low priority |

### Templates

| Method | Description |
|--------|-------------|
| `UsingTemplate(template, model, isHtml?)` | Renders an inline template |
| `UsingTemplateFromFile(path, model, isHtml?)` | Renders a template file |
| `UsingTemplateFromEmbedded(path, model, assembly, isHtml?)` | Renders an embedded resource |
| `UsingCultureTemplateFromFile(path, model, culture, isHtml?)` | Renders a culture-specific template |
| `UsingTemplateEngine(renderer)` | Replaces the template renderer |
| `PlaintextAlternativeUsingTemplate(...)` | Plain-text alternative variants |

### Attachments and metadata

| Method | Description |
|--------|-------------|
| `Attach(attachment)` | Adds an attachment |
| `Attach(attachments)` | Adds multiple attachments |
| `AttachFromFilename(path, contentType?, name?)` | Adds an attachment from a file |
| `Tag(tag)` | Adds a tag (supported providers only) |
| `Header(name, value)` | Adds a custom header |

### Send

| Method | Description |
|--------|-------------|
| `Send(token?)` | Sends synchronously |
| `SendAsync(token?)` | Sends asynchronously |

## `Email`

Concrete implementation of `IDotRMail`.

```csharp
// Static factory
Email.From("from@example.com", "Name");

// Constructors
new Email();
new Email(renderer, sender);
new Email("from@example.com", "Name");
new Email(renderer, sender, "from@example.com", "Name");

// Configurable defaults
Email.DefaultRenderer = new ReplaceRenderer();
Email.DefaultSender = new InMemorySender();
```

## Models

### `EmailData`

Contains all email data: recipients, subject, body, attachments, headers, and tags.

### `Address`

```csharp
new Address("user@example.com");
new Address("user@example.com", "Name");
address.ToString(); // "Name <user@example.com>"
```

### `Attachment`

| Property | Description |
|----------|-------------|
| `Filename` | File name |
| `Data` | Content stream |
| `ContentType` | MIME type |
| `IsInline` | Inline attachment (CID) |
| `ContentId` | Content ID for inline attachments |

### `SendResponse`

| Property | Description |
|----------|-------------|
| `Successful` | `true` when there are no errors |
| `MessageId` | ID returned by the provider |
| `ErrorMessages` | List of errors |

### `Priority`

Enum: `High`, `Normal`, `Low`.

## Abstractions (`DotRMail.Core.Abstractions`)

Contracts and extensible interfaces for the library.

### `ISender`

Implement to create custom send providers (SendGrid, Mailgun, etc.).

```csharp
public interface ISender
{
    SendResponse Send(IDotRMail email, CancellationToken? token = null);
    Task<SendResponse> SendAsync(IDotRMail email, CancellationToken? token = null);
}
```

### `ITemplateRenderer`

Implement for custom template engines (Razor, Handlebars, etc.).

```csharp
public interface ITemplateRenderer
{
    string Parse<T>(string template, T model, bool isHtml = true);
    Task<string> ParseAsync<T>(string template, T model, bool isHtml = true);
}
```

## Built-in providers

### Core — `DotRMail.Core.Defaults`

| Class | Purpose |
|-------|---------|
| `ReplaceRenderer` | Replaces `##Property##` tokens |
| `InMemorySender` | Captures emails in memory (testing) |
| `SaveToDiskSender` | Persists emails to disk (debugging) |

### SMTP — `DotRMail.Smtp`

| Class | Purpose |
|-------|---------|
| `SmtpSender` | Delivery via `SmtpClient` |

### Liquid — `DotRMail.Liquid`

| Class | Purpose |
|-------|---------|
| `LiquidRenderer` | Liquid rendering via Fluid |
| `LiquidRendererOptions` | Configuration options |

## Dependency injection

```csharp
// Microsoft.Extensions.DependencyInjection
services.AddDotRMail("from@example.com", "Name")
    .AddSmtpSender("localhost", 25)
    .AddLiquidRenderer();

// Registered services:
// - IDotRMail (transient)
// - IDotRMailFactory (transient)
// - ISender (singleton, via extension)
// - ITemplateRenderer (singleton, via extension)
```

### `IDotRMailFactory`

```csharp
public interface IDotRMailFactory
{
    IDotRMail Create();
}
```

## Configuration extensions

### `AddSmtpSender`

```csharp
.AddSmtpSender()                              // default SmtpClient
.AddSmtpSender("host", 587)                   // host + port
.AddSmtpSender(() => new SmtpClient { ... })   // custom factory
```

### `AddLiquidRenderer`

```csharp
.AddLiquidRenderer()
.AddLiquidRenderer(options => options.FileProvider = new PhysicalFileProvider("./Templates"))
```
