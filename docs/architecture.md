# Architecture — DotRMail

Overview of the architecture and design patterns used in the library.

## Overview

DotRMail follows a modular architecture inspired by FluentEmail, separating responsibilities into three layers:

```
┌─────────────────────────────────────────────────────────┐
│                    Consumer application                 │
│         (ASP.NET Core, Worker, Console, Tests)          │
└─────────────────────────┬───────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                     DotRMail.Core                       │
│  ┌──────────── Abstractions/ ──────────────┐            │
│  │ IDotRMail │ ITemplateRenderer │ ISender │            │
│  └─────────────────────────────────────────┘            │
│  ┌──────────┐                    ┌──────────────────┐   │
│  │  Email   │                    │ ReplaceRenderer  │   │
│  └──────────┘                    │ InMemorySender   │   │
│                                  │ SaveToDiskSender │   │
│                                  └──────────────────┘   │
└──────────────┬──────────────────────────┬───────────────┘
               │                          │
               ▼                          ▼
    ┌──────────────────┐      ┌──────────────────┐
    │  DotRMail.Liquid │      │   DotRMail.Smtp  │
    │  LiquidRenderer  │      │    SmtpSender    │
    └──────────────────┘      └──────────────────┘
```

## Design principles

### 1. Fluent interface

The fluent pattern enables readable, chainable composition:

```csharp
email.To("user@example.com")
     .Subject("Subject")
     .Body("Body")
     .SendAsync();
```

Each configuration method returns `IDotRMail` to preserve chaining.

### 2. Strategy pattern

`ISender` and `ITemplateRenderer` are interchangeable strategies:

- **Delivery**: SMTP, disk, in-memory, or a custom implementation
- **Templates**: placeholders, Liquid, or a custom engine

The application configures strategies via DI without changing business code.

### 3. Factory

`IDotRMailFactory` supports sending multiple independent emails within the same DI scope, avoiding shared state between transient instances.

### 4. Separation of concerns

| Package | Responsibility |
|---------|----------------|
| `DotRMail.Core` | Domain model, fluent API, abstractions, and defaults |
| `DotRMail.Smtp` | SMTP integration |
| `DotRMail.Liquid` | Fluid (Liquid) integration |

Integration packages depend only on Core, never on each other.

## Send flow

```
1. User configures the email via the fluent API
         │
         ▼
2. (Optional) ITemplateRenderer.Parse() generates the body
         │
         ▼
3. EmailData populated with recipients, subject, body, attachments
         │
         ▼
4. ISender.SendAsync() delivers the message
         │
         ▼
5. SendResponse returned to the caller
```

## Data model

`EmailData` is the aggregate root that holds email state:

- Mutable collections for recipients, attachments, tags, and headers
- Scalar properties for subject, body, and metadata
- `Address` reference for the sender

Senders receive `IDotRMail` (not `EmailData` directly) for full context access.

## Dependency injection

```
AddDotRMail(from, name)
    ├── IDotRMail → Email (transient)
    │       ├── ITemplateRenderer (optional, singleton)
    │       └── ISender (optional, singleton)
    └── IDotRMailFactory → DotRMailFactory (transient)
```

Extensions such as `AddSmtpSender()` and `AddLiquidRenderer()` register concrete implementations with `TryAdd`, allowing overrides without conflicts.

## Testability

| Component | Testing strategy |
|-----------|------------------|
| Fluent API | Direct tests in `EmailBuilderTests` |
| Templates | Isolated `ReplaceRenderer` and `LiquidRenderer` tests |
| Delivery | `InMemorySender` captures `EmailData` without I/O |
| DI | `ServiceCollection` + `BuildServiceProvider()` |
| Disk | `SaveToDiskSender` with a temporary directory |

## Extensibility

To add a new provider (e.g. SendGrid):

1. Create a `DotRMail.SendGrid` project
2. Implement `ISender`
3. Add an `AddSendGridSender(this DotRMailServicesBuilder builder, ...)` extension
4. Reference only `DotRMail.Core`

To add a renderer (e.g. Razor):

1. Create a `DotRMail.Razor` project
2. Implement `ITemplateRenderer`
3. Add an `AddRazorRenderer(...)` extension

## Technical decisions

| Decision | Rationale |
|----------|-----------|
| .NET 8.0 | Modern LTS with broad support |
| Native `SmtpClient` | Zero extra dependencies in the SMTP package |
| Fluid for Liquid | Mature engine compatible with Shopify Liquid |
| `ReplaceRenderer` in Core | Simple templates without external dependencies |
| XML docs on all packages | Professional IntelliSense out of the box |
| xUnit for tests | Common standard in the .NET ecosystem |

## Suggested roadmap

- [ ] `DotRMail.Razor` — Razor templates via RazorLight
- [ ] `DotRMail.SendGrid` — SendGrid API integration
- [ ] `DotRMail.Mailgun` — Mailgun API integration
- [ ] Publish to NuGet.org
- [ ] Examples in `samples/`
