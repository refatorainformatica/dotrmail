# DotRMail

A .NET library for fluent email composition and delivery, inspired by [FluentEmail](https://github.com/lukencode/FluentEmail).

DotRMail provides a chainable API, template support, dependency injection, and extensible providers — ideal for ASP.NET Core apps, workers, and background services.

## Features

- **Fluent API** — compose emails with chainable methods
- **Templates** — rendering with placeholders (`ReplaceRenderer`) or Liquid (`DotRMail.Liquid`)
- **Send providers** — SMTP, disk (debug), and in-memory (testing)
- **Dependency injection** — native integration with `Microsoft.Extensions.DependencyInjection`
- **Testable** — `InMemorySender` captures emails in unit tests
- **XML documentation** — full IntelliSense across all public packages

## Packages

| Package | Description |
|---------|-------------|
| `DotRMail.Core` | Domain model, fluent API, and defaults |
| `DotRMail.Smtp` | SMTP delivery via `System.Net.Mail.SmtpClient` |
| `DotRMail.Liquid` | Liquid template rendering (Fluid) |

## Installation

```bash
dotnet add package DotRMail.Core
dotnet add package DotRMail.Smtp
dotnet add package DotRMail.Liquid
```

> Packages are available on [NuGet.org](https://www.nuget.org/packages?q=DotRMail).

## Quick start

### Simple send

```csharp
using DotRMail.Core;
using DotRMail.Core.Defaults;

var sender = new InMemorySender();

await Email.From("noreply@company.com", "My Company")
    .To("customer@email.com", "Customer")
    .Subject("Welcome!")
    .Body("<h1>Hello!</h1><p>Thanks for signing up.</p>", isHtml: true)
    .SendAsync();
```

### With dependency injection (ASP.NET Core)

```csharp
// Program.cs
builder.Services
    .AddDotRMail("noreply@company.com", "My Company")
    .AddSmtpSender("smtp.company.com", 587)
    .AddLiquidRenderer();

// NotificationService.cs
public class NotificationService(IDotRMail email)
{
    public async Task SendWelcomeAsync(string recipient, string name)
    {
        await email
            .To(recipient, name)
            .Subject("Welcome to DotRMail")
            .UsingTemplate("Hello {{ name }}, welcome aboard!", new { name })
            .SendAsync();
    }
}
```

### Placeholder templates

```csharp
await email
    .To("user@example.com")
    .Subject("Password reset")
    .UsingTemplate("Hello ##Name##, your code is ##Code##.", new { Name = "John", Code = "123456" })
    .SendAsync();
```

## Run tests

```bash
dotnet test
```

## Documentation

- [Getting started](docs/getting_started.md)
- [API reference](docs/api.md)
- [Architecture](docs/architecture.md)

## License

See [LICENSE](LICENSE).
