# AGENTS.md

This guidance is for agentic coding tools working in this repository.
Keep changes aligned with existing patterns and test behaviors.

## Quick Context

- Project: RAIT (REST API Integration Testing), a C# library for typed REST API tests.
- Core uses expression trees to turn controller method calls into HTTP requests.
- Primary library: `RAIT.Core/` (net8.0).
- Example API and tests live under `RAIT.Example.API*/`.
- Tests use NUnit and `WebApplicationFactory<Program>`.

## Build, Lint, Test

Run commands from repo root.

```bash
# Restore dependencies
dotnet restore

# Build (Release)
dotnet build --configuration Release --no-restore

# Run all tests
dotnet test --no-restore --verbosity normal

# Run a specific test class (filter by fully qualified name)
dotnet test --filter "FullyQualifiedName~RaitCoreTests"

# Run a single test
dotnet test --filter "FullyQualifiedName~RaitCoreTests.GetWithId_ValidId_ReturnsExpectedResult"

# Pack for NuGet
dotnet pack --no-build --configuration Release
```

Linting:
- No dedicated lint/format command found in the repo.
- If you add one, document it here and keep consistent with .NET conventions.

## Repository Structure

- `RAIT.Core/`: main library (net8.0), published as NuGet package "RAIT".
- `RAIT.Example.API/`: sample API with multiple controllers and scenarios.
- `RAIT.Example.API.Test/`: NUnit tests for core RAIT behaviors.
- `RAIT.Example.API.Endpoints/` and `RAIT.Example.API.Endpoints.Test/`:
  Ardalis.ApiEndpoints integration and tests.
- `RAIT.Example.API.ArdalisApiEndpoints*/`: legacy/alternate endpoint examples.

## Code Style Guidelines

General C# style (observed in codebase):

- Use file-scoped namespaces (`namespace Foo;`).
- Put `using` directives at top; group in this order:
  1) `System.*`, 2) `Microsoft.*`, 3) project/third-party (e.g., `RAIT.*`).
- Use 4-space indentation; braces on new lines.
- Prefer one statement per line; keep lines readable.
- Use `var` when the type is obvious from the right-hand side.
- Use explicit types when clarity matters (public APIs, complex generics).
- Use `async`/`await` for async flows; return `Task` or `Task<T>`.
- Apply nullable annotations and null-forgiving (`null!`) where initialization is deferred.

Naming conventions:

- Types, methods, properties: PascalCase.
- Parameters, locals: camelCase.
- Private fields: `_camelCase` (underscore prefix is used in tests and core).
- Generic type parameters: `T`, `TOutput`, `TController`, `TOut`.
- Test methods follow `Action_Scenario_Expected` (NUnit-friendly).

Formatting patterns observed:

- Object initializers use multi-line braces for multi-property setup.
- `#region` blocks appear in core APIs, but preference is to remove them when touched.
- Use blank lines to separate logical blocks and between methods.

## Error Handling and Nullability

- Use exceptions for invalid states; `RaitHttpException` is thrown on non-success HTTP responses.
- Prefer guard clauses for early exits (e.g., null checks before use).
- When a non-null response is required, throw `ArgumentNullException` as in `CallRequired`/`CallRequiredAsync` (`CallR` alias).
- Avoid swallowing exceptions; tests assert exceptions explicitly with `Assert.ThrowsAsync`.

## API and Architecture Notes

- `RaitHttpClientWrapper<TController>` is the entry point via `HttpClient.Rait<T>()`.
  - `Call()` / `CallAsync()` return deserialized response (nullable).
  - `CallRequired()` / `CallRequiredAsync()` return response with null check (`CallR` alias).
  - `CallHttp()` / `CallHttpAsync()` return raw `HttpResponseMessage` (`CallH` alias).
- `RequestPreparer<TController>` parses expression trees to extract method info and params.
- `RaitRouter` and related classes build routes and query strings.
- `RaitParameterExtractor` distinguishes `[FromQuery]`, `[FromBody]`, `[FromForm]`, `[FromHeader]`.

## Testing Conventions

- NUnit attributes: `[Test]`, `[SetUp]`, `[TearDown]`.
- Base test setup uses `WebApplicationFactory<Program>`.
- Common pattern:
  - Configure services with `AddRait()`.
  - Call `Services.ConfigureRait()` after building the test host.
- Use `Assert.That` for assertions and `Assert.ThrowsAsync` for error cases.
- Keep tests deterministic; set `CultureInfo` explicitly when validating formatting.

## Serialization

- Default serializer is `System.Text.Json`.
- If Newtonsoft.Json or custom options are needed, register via `services.AddRait()`
  and call `Services.ConfigureRait()`.

## Cursor / Copilot Rules

- No `.cursor/rules/`, `.cursorrules`, or `.github/copilot-instructions.md` found.
- If these are added later, update this file with their guidance.

## What to Preserve

- Public API signatures and behaviors (this is a library).
- Expression-tree based routing and parameter extraction flows.
- Existing test naming and setup patterns.
- Existing exception behaviors and error messages where tests depend on them.

## When Adding New Code

- Favor small, targeted changes with clear unit/integration tests.
- Avoid adding new `#region` blocks; remove existing regions if you are editing those sections.
- Prefer minimal public surface area unless a new API is required.
- Update tests when behavior changes are intentional.
- Update this file if build/test commands or style rules change.
