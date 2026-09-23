# Repository Guidelines

## Project Structure & Module Organization

`Legend2Toolbox.slnx` groups the .NET 8 projects. Backend code lives under `src/`: `Domain` contains entities and shared business types, `Application` holds features and CQRS handlers, `Infrastructure` implements persistence and external services, and `Api` exposes endpoints and SignalR hubs. `WpfClient` is the Windows desktop client; `Shared` contains cross-client helpers. The Vue 3/Vite application is in `src/Legend2toolbox.web`; follow its nested `AGENTS.md` for frontend-specific guidance. Unit and API integration tests are under `tests/`. EF Core migrations belong in `src/Legend2Toolbox.Infrastructure/Migrations`.

## Build, Test, and Development Commands

- `dotnet build Legend2Toolbox.slnx` restores and builds the .NET solution.
- `dotnet test Legend2Toolbox.slnx` runs all xUnit suites; integration tests require Docker for the PostgreSQL Testcontainer.
- `dotnet run --project src/Legend2Toolbox.Api` starts the API locally.
- `dotnet run --project src/Legend2Toolbox.WpfClient` launches the Windows desktop client.
- In `src/Legend2toolbox.web`, run `npm ci`, then `npm run dev` for Vite development. Use `npm run build`, `npm run type-check`, and `npm run lint` before submitting frontend changes.

## Coding Style & Naming Conventions

Use four-space indentation in C# and existing file-scoped namespaces. Keep nullable reference types enabled. Name public types and members in PascalCase, locals and parameters in camelCase, interfaces with an `I` prefix, and asynchronous methods with an `Async` suffix. Feature types should retain the established suffixes (`Command`, `Query`, `Handler`, `Validator`, `Endpoints`). Frontend files follow `.editorconfig`: two spaces, LF endings, and a 100-character line limit. Do not hand-edit `src/api/generated`; regenerate it with `npm run api:sync`.

## Testing Guidelines

Tests use xUnit and FluentAssertions; unit tests also use NSubstitute, while integration tests use `WebApplicationFactory` and Testcontainers. Name files `<Subject>Tests.cs` and tests descriptively, such as `Validator_ShouldHaveError_WhenPasswordIsInvalid`. Add focused tests with every behavior change. No coverage threshold is enforced; collect coverage with `dotnet test --collect:"XPlat Code Coverage"` when needed.

## Commit & Pull Request Guidelines

Recent commits use brief PascalCase action summaries such as `AddScriptManagement` and `FinishedCardsView`; keep commits focused and similarly concise. Pull requests should explain the change and validation performed, link relevant issues, include screenshots for Vue/WPF UI changes, and call out migrations, generated API updates, or configuration changes.

## Security & Configuration

Never commit credentials, tokens, or production connection strings. Use .NET user secrets or environment variables for local overrides, and keep committed `appsettings*.json` values non-sensitive.

## Agent Workflow

For non-trivial changes:

- Inspect the relevant code before editing.
- Establish the actual execution path.
- Identify the rot cause before modifying code.
- Make the smallest coherent change.
- Modify only relevant files.
- Verify the result. 

Do not guess when repository evidence can confirm the answer.

## verification

- Run relevant tests when available.
- Run dotnet build or npm run build.
- Report failed verification.
- Do not claim completion when verification fails.
- Summarize changed files and verification performed.
