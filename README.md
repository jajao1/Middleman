# <img src="logo.png" alt="Middleman Logo" width="48" height="48" align="center"> Middleman 🤝

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Middleman is a lightweight, in-process mediator library for .NET focused on CQRS-style request/query dispatching and notification (event) publication.

## Installation

```bash
dotnet add package Middleman
```

## Target Frameworks

- Middleman: net9.0, netstandard2.0
- Middleman.FluentValidation: net9.0

## Core Concepts

- `IMiddleman`: Main dispatcher interface (send, publish, stream).
- `IRequest<TResponse>`: Request/query with a single response.
- `IRequest`: Request with no response.
- `IStreamRequest<TResponse>`: Request/query that returns `IAsyncEnumerable<TResponse>`.
- `INotification`: Marker for published events/notifications.

## Quick Start

### 1) Define Contracts

```csharp
using Middleman;

public sealed class CreateUserRequest : IRequest<Guid>
{
    public CreateUserRequest(string name) => Name = name;
    public string Name { get; }
}

public sealed class UserCreatedNotification : INotification
{
    public UserCreatedNotification(Guid userId) => UserId = userId;
    public Guid UserId { get; }
}
```

### 2) Implement Handlers

```csharp
using Middleman;

public sealed class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, Guid>
{
    private readonly IMiddleman _middleman;

    public CreateUserRequestHandler(IMiddleman middleman)
    {
        _middleman = middleman;
    }

    public async Task<Guid> Handle(CreateUserRequest message, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        await _middleman.Publish(new UserCreatedNotification(id), cancellationToken);
        return id;
    }
}

public sealed class UserCreatedLogger : INotificationHandler<UserCreatedNotification>
{
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"User created: {notification.UserId}");
        return Task.CompletedTask;
    }
}
```

### 3) Register in DI

```csharp
using System.Reflection;
using Middleman;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMiddleman(Assembly.GetExecutingAssembly(), options =>
{
    options.PublishStrategy = NotificationPublishStrategy.Parallel;
    options.ContinueOnException = false;
});

// Optional: enable in-memory request response caching
builder.Services.AddMiddlemanCaching();
```

### 4) Dispatch

```csharp
var id = await middleman.Send(new CreateUserRequest("joao"), cancellationToken);
```

## Release Notes (March 2026)

### Added

- Core package multi-target support for net9.0 and netstandard2.0

- Centralized exception handling behavior
  - `IExceptionHandler<TRequest, TResponse>`
  - `IExceptionHandler<TRequest>`
  - `ExceptionHandlingBehavior<TRequest, TResponse>`
  - `ExceptionHandlingBehavior<TRequest>`
- Response caching behavior for requests
  - `ICacheableRequest<TResponse>`
  - `CachingBehavior<TRequest, TResponse>`
  - `AddMiddlemanCaching()`
- Configurable notification publish strategies
  - `MiddlemanOptions`
  - `NotificationPublishStrategy.Parallel`
  - `NotificationPublishStrategy.Sequential`
  - `ContinueOnException`
- Async stream request support
  - `IStreamRequest<TResponse>`
  - `IStreamRequestHandler<TRequest, TResponse>`
  - `CreateStream<TResponse>(...)` on sender/dispatcher

### Internal Improvements

- Safer reflective invocation for handlers and pipeline behaviors.
- Expanded xUnit coverage for exception handling, caching, publish strategy, and streaming.

## API Overview

| Interface | Description |
| :--- | :--- |
| `IMiddleman` | Central dispatcher (send, publish, stream). |
| `ISender` | Send requests and create streams. |
| `IPublisher` | Publish notifications/events. |
| `IRequest<TResponse>` | Request/query with response. |
| `IRequest` | Request without response. |
| `IStreamRequest<TResponse>` | Stream request/query with async stream response. |
| `INotification` | Notification/event marker. |
| `IRequestHandler<,>` / `IRequestHandler<>` | Request handlers. |
| `IStreamRequestHandler<,>` | Stream request handlers. |
| `INotificationHandler<TNotification>` | Notification handlers. |

## Contributing

Contributions are welcome. Open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See `LICENSE` for details.