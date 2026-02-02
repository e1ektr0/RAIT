# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RAIT (REST API Integration Testing) is a C# library for strongly typed REST API testing. It uses expression trees to convert C# method calls into HTTP requests, providing compile-time type safety and IDE navigation support.

## Build and Test Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release --no-restore

# Run all tests
dotnet test --no-restore --verbosity normal

# Run a specific test file
dotnet test --filter "FullyQualifiedName~RaitTests"

# Run a single test
dotnet test --filter "FullyQualifiedName~RaitTests.Post_ValidModel_ReturnsExpectedResult"

# Pack for NuGet
dotnet pack --no-build --configuration Release
```

## Architecture

### Core Components (RAIT.Core/)

The library works by intercepting C# lambda expressions and translating them to HTTP requests:

1. **RaitHttpClientWrapper<TController>** - Main entry point via `HttpClient.Rait<T>()` extension
   - `Call()` / `CallAsync()` - Returns deserialized response
   - `CallRequired()` / `CallRequiredAsync()` - Returns response with null check (`CallR` alias)
   - `CallHttp()` / `CallHttpAsync()` - Returns raw HttpResponseMessage (`CallH` alias)

2. **RequestPreparer<TController>** - Parses lambda expressions, extracts method info and parameters

3. **RaitRouter** - Builds routes from controller/method attributes, handles route templates and query strings

4. **RaitParameterExtractor** - Distinguishes between `[FromQuery]`, `[FromBody]`, `[FromForm]`, `[FromHeader]` parameters

5. **RaitHttpRequester<TController>** - Executes HTTP requests, handles response deserialization

### Test Setup Pattern

Tests use `WebApplicationFactory<Program>` with RAIT registered in DI:

```csharp
[SetUp]
public void Setup()
{
    _application = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(s => s.AddRait());
            builder.UseEnvironment("Test");
        });
    _defaultClient = _application.CreateDefaultClient();
    _application.Services.ConfigureRait();
}
```

### Serialization

Default: System.Text.Json. For Newtonsoft.Json or custom settings, register via `services.AddRait()` and call `Services.ConfigureRait()`.

## Project Structure

- **RAIT.Core/** - Main library (net8.0), published to NuGet as "RAIT"
- **RAIT.Example.API/** - Test API with 18+ controllers covering various scenarios
- **RAIT.Example.API.Test/** - NUnit tests demonstrating library usage
- **RAIT.Example.API.Endpoints/** - Ardalis.ApiEndpoints integration (net8.0)
- **RAIT.Example.API.Endpoints.Test/** - Tests for Ardalis endpoints support

## Key Implementation Details

- Expression trees are parsed to extract controller type, method info, and parameter values
- Route building handles `[controller]`, `[action]` placeholders and route parameters
- `InternalsVisibleTo` exposes internals to test project
- Custom `RaitHttpException` includes HTTP status code for error handling
- Cookies are automatically managed between requests
