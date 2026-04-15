# AGENTS.md

## Repo Shape

- `RAIT.Core/` is the only packable library project (`PackageId=RAIT`, currently `Version=3.0.0`). Treat public API shape and exception behavior as release-sensitive.
- `RAIT.Example.API/` and `RAIT.Example.API.Test/` target `net9.0`.
- `RAIT.Example.API.Endpoints/` and `RAIT.Example.API.Endpoints.Test/` target `net8.0`.
- Local work needs both .NET 8 and .NET 9 SDKs. `.github/workflows/action.yml` still installs 8.0 and 6.0, so do not copy its SDK list as the source of truth.

## Commands

- Run from repo root: `dotnet restore`
- Build all projects: `dotnet build --configuration Release --no-restore`
- Run all tests: `dotnet test --no-restore --verbosity normal`
- Run one core test: `dotnet test RAIT.Example.API.Test/RAIT.Example.API.Test.csproj --filter "FullyQualifiedName~RaitCoreTests.GetWithId_ValidId_ReturnsExpectedResult"`
- Run endpoints tests only: `dotnet test RAIT.Example.API.Endpoints.Test/RAIT.Example.API.Endpoints.Test.csproj`
- Pack the NuGet package: `dotnet pack RAIT.Core/RAIT.Core.csproj --configuration Release --no-build`

## What Actually Wires RAIT

- `HttpClient.Rait<TController>()` in `RAIT.Core/RaitHttpClientExtensions.cs` is the entry point and requires `TController : ControllerBase`.
- Request construction flows through `RequestPreparer` -> `RaitParameterExtractor` -> `RaitRouter`. Route-template, query-string, and expression-evaluation regressions usually belong in those files.
- `ServiceCollectionExtensions.ConfigureRait()` must run after `services.AddRait()` and after the `WebApplicationFactory` has been built; otherwise serialization/service-locator setup is incomplete.

## Test Setup Quirks

- `RAIT.Example.API.Test/Infrastructure/RaitTestBase.cs` only sets `UseEnvironment("Test")`. Use it for tests that do not need DI-backed RAIT configuration.
- `RAIT.Example.API.Test/Infrastructure/RaitConfiguredTestBase.cs` adds `services.AddRait()` in `ConfigureTestServices` and then calls `Services.ConfigureRait()` in `Setup()`. Follow that pattern for typed-results or serializer-dependent tests.
- `RAIT.Example.API.Endpoints.Test/Infrastructure/EndpointsTestBase.cs` always configures RAIT; endpoints tests assume that wiring exists.
- If you change culture-sensitive query serialization, rerun `RaitCoreTests.DateOnly_query_parameters_are_serialized_invariantly`.

## Serialization And Docs

- The plain example API uses default ASP.NET Core controller serialization; the endpoints sample explicitly uses `AddNewtonsoftJson(...)` with camel-case settings. Verify serializer changes against both sample apps.
- Both sample apps enable XML docs and call `IncludeRaitXml()`. The `RAIT/` subfolders under the sample apps contain generated documentation/example artifacts; prefer regenerating them through code/tests rather than hand-editing.

## CI / Release Gotcha

- `.github/workflows/action.yml` runs restore, build, test, pack, then `dotnet nuget push **/*.nupkg` on every push. Changes to `RAIT.Core.csproj` package metadata/version or pack output are release-impacting.
