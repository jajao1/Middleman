# Middleman.FluentValidation 🛡️✨

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

The official extension to integrate [FluentValidation](https://docs.fluentvalidation.net/en/latest/) into your [Middleman](https://github.com/jajao1/Middleman) request and notification pipeline. Ensure your requests and queries are always validated before being processed by their respective handlers.

## Overview

`Middleman.FluentValidation` provides `IPipelineBehavior` implementations that intercept `IRequest` and `IRequest<TResponse>` dispatched through `IMiddleman`. It automatically locates and executes all registered FluentValidation validators (`AbstractValidator<TRequest>`) for the given request. If any validation rule fails, a `FluentValidation.ValidationException` is thrown, preventing the request from reaching its handler.

This library is essential for implementing robust and decoupled validation within your Middleman-based architecture.

## Installation

The easiest way to add `Middleman.FluentValidation` to your project is via the NuGet Package Manager.

```bash
dotnet add package Middleman.FluentValidation
```

You will also need to have `Middleman` and `FluentValidation.DependencyInjectionExtensions` installed in your application project:

```bash
dotnet add package Middleman
dotnet add package FluentValidation.DependencyInjectionExtensions
```

## Configuration

Configure `Middleman.FluentValidation` in your `Program.cs` (or `Startup.cs` for .NET Framework) alongside the registration of Middleman and your validators.

```csharp
using System.Reflection;
using FluentValidation;
using Middleman; // For AddMiddleman
using Middleman.FluentValidation; // For AddMiddlemanFluentValidation

var builder = WebApplication.CreateBuilder(args);

// ... other services

// 1. Register Middleman and all its handlers within the current assembly
builder.Services.AddMiddleman(Assembly.GetExecutingAssembly());

// 2. Add the Validation Pipeline Behavior to Middleman
//    This ensures all requests will go through validation
builder.Services.AddMiddlemanFluentValidation(Assembly.GetExecutingAssembly());

// ... rest of your application setup and build
```

## Example Usage

### 1. Define Your Requests and Validators

Create your requests (with or without a return value) and their respective validators inheriting from `AbstractValidator<TRequest>`.

```csharp
// Request with a return value
public class CreateUserRequest : IRequest<Guid>
{
    public string UserName { get; }
    public string Email { get; }
    public CreateUserRequest(string userName, string email) { UserName = userName; Email = email; }
}

// Validator for CreateUserRequest
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required.")
            .MinimumLength(3).WithMessage("User name must be at least 3 characters long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}

// Request without a return value
public class DeleteUserRequest : IRequest
{
    public Guid UserId { get; }
    public DeleteUserRequest(Guid userId) => UserId = userId;
}

// Validator for DeleteUserRequest
public class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
{
    public DeleteUserRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID cannot be empty.");
    }
}
```

### 2. Dispatch Your Requests via IMiddleman

In your application logic (e.g., in an API Controller), inject `IMiddleman` and dispatch your requests. Validation will be executed automatically.

```csharp
using Microsoft.AspNetCore.Mvc;
using Middleman.Abstractions;
using FluentValidation; // To catch ValidationException
using System.Linq; // For Select extension method

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMiddleman _middleman;

    public UsersController(IMiddleman middleman)
    {
        _middleman = middleman;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var request = new CreateUserRequest(dto.UserName, dto.Email);
        try
        {
            var newUserId = await _middleman.Send(request); // Validation occurs here!
            return Ok(new { Id = newUserId });
        }
        catch (ValidationException ex)
        {
            // Catch validation failures and return a Bad Request
            return BadRequest(ex.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var request = new DeleteUserRequest(id);
        try
        {
            await _middleman.Send(request); // Validation occurs here!
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }));
        }
    }
}
```

## How It Works (Internally)

`Middleman.FluentValidation` registers two distinct `IPipelineBehavior` implementations:

* `ValidationBehavior<TRequest, TResponse>`: Used for requests that implement `IRequest<TResponse>`.
* `ValidationBehaviorNoResult<TRequest>`: Used for requests that implement only `IRequest`.

Both behaviors intercept the request, resolve all `IValidator<TRequest>` for that specific request from the DI container, and execute them. If any failures are found, a `ValidationException` is thrown, halting the pipeline's execution.

## Contributing

Contributions are welcome! If you find a bug or have a suggestion for an improvement, feel free to open an issue or submit a Pull Request.

## License

This project is licensed under the **MIT License**. See the `LICENSE` file for more details.
