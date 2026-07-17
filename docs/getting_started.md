# Getting started — DotRMail

This guide covers setup and the most common DotRMail use cases.

## Prerequisites

- .NET 8.0 SDK or later
- Basic C# knowledge and, optionally, ASP.NET Core

## 1. Reference the packages

Add the required projects or packages to your solution:

```bash
dotnet add reference ../src/DotRMail.Core/DotRMail.Core.csproj
dotnet add reference ../src/DotRMail.Smtp/DotRMail.Smtp.csproj
```

## 2. Send without DI (standalone)

For scripts, tests, or quick prototypes:

```csharp
using DotRMail.Core;
using DotRMail.Core.Defaults;

var sender = new InMemorySender();

var email = Email.From("from@test.com")
    .To("to@test.com")
    .Subject("Test")
    .Body("Message body");

email.Sender = sender;
email.Renderer = new ReplaceRenderer();

var response = await email.SendAsync();

if (response.Successful)
{
    Console.WriteLine($"Sent! ID: {response.MessageId}");
}
```

## 3. Configure with DI

### ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDotRMail(
        defaultFromEmail: builder.Configuration["Email:From"]!,
        defaultFromName: builder.Configuration["Email:FromName"]!)
    .AddSmtpSender(
        builder.Configuration["Email:Smtp:Host"]!,
        int.Parse(builder.Configuration["Email:Smtp:Port"]!))
    .AddLiquidRenderer();

var app = builder.Build();
```

### appsettings.json

```json
{
  "Email": {
    "From": "noreply@company.com",
    "FromName": "Company",
    "Smtp": {
      "Host": "smtp.company.com",
      "Port": 587
    }
  }
}
```

## 4. Multiple emails in the same request

Use `IDotRMailFactory` when you need to send several independent emails:

```csharp
public class BatchService(IDotRMailFactory factory)
{
    public async Task SendBatchAsync(IEnumerable<string> recipients)
    {
        foreach (var recipient in recipients)
        {
            await factory.Create()
                .To(recipient)
                .Subject("Notification")
                .Body("Batch message")
                .SendAsync();
        }
    }
}
```

## 5. Templates

### Placeholders (`ReplaceRenderer`)

Replaces tokens in the format `##Property##`:

```csharp
.UsingTemplate("Hello ##Name##!", new { Name = "Maria" })
```

### Liquid (`DotRMail.Liquid`)

Supports full Liquid syntax via the Fluid library:

```csharp
.UsingTemplate("Hello {{ user.name }}, your plan is {{ user.plan }}.", new
{
    user = new { name = "Carlos", plan = "Premium" }
})
```

### Template file

```csharp
.UsingTemplateFromFile("./Templates/welcome.liquid", new { Name = "Anna" })
```

### Embedded resource template

```csharp
.UsingTemplateFromEmbedded(
    "MyApp.Templates.Email.txt",
    new { Name = "Peter" },
    typeof(MyApp.Templates).Assembly)
```

## 6. Attachments

```csharp
.Attach(new Attachment
{
    Filename = "report.pdf",
    ContentType = "application/pdf",
    Data = File.OpenRead("./report.pdf")
})

// or

.AttachFromFilename("./report.pdf", "application/pdf", "Monthly Report.pdf")
```

## 7. Plain-text alternative body

Improves compatibility and deliverability when the main body is HTML:

```csharp
.Body("<h1>Hello</h1><p>HTML content</p>", isHtml: true)
.PlaintextAlternativeBody("Hello\nPlain-text content")
```

## 8. Unit testing

Use `InMemorySender` to verify emails without real delivery:

```csharp
var sender = new InMemorySender();
var email = new Email(new ReplaceRenderer(), sender)
    .SetFrom("test@test.com")
    .To("user@test.com")
    .Subject("Test")
    .Body("Body");

await email.SendAsync();

Assert.Single(sender.SentEmails);
Assert.Equal("Test", sender.SentEmails[0].Subject);
```

## 9. Local debugging

`SaveToDiskSender` persists emails as `.eml` files:

```csharp
Email.DefaultSender = new SaveToDiskSender("/tmp/dotrmail");
```

## Next steps

- [API reference](API.md)
- [Architecture](ARCHITECTURE.md)
