# .NET Modular Monolith Template

A generalized .NET modular monolith template following Clean Architecture with module boundaries, CQRS, FluentValidation, Mapperly, MongoDB with Outbox/Inbox eventing, and JWT-based auth.

## Features
- Modular monolith structure with strict boundaries (Domain/Application/Infrastructure/API)
- CQRS-style Commands/Queries; FluentValidation validators; Mapperly mappers
- Shared kernel for cross-cutting primitives (Results, Outbox/Inbox DTOs)
- MongoDB repositories with index configurators and seeders
- JWT auth (policies preferred over roles)
- Outbox/Inbox design for eventing (no external broker)

## Quick start
1) Create a repository from this template:
   - GitHub UI: Use this template → create your repo
   - CLI: `gh repo create <owner>/<repo> --template MaiQD/dotnet-modular-monolith-template --public`
2) Optional: rename solution/root namespace
   - Run: `scripts/rename.sh MyCompany.MyApp MyApp.Monolith`
   - First arg: new root namespace; second arg (optional): new solution name (defaults to first)
3) Add a module skeleton:
   - `scripts/new-module.sh Inventory MyCompany.MyApp`
   - This creates Domain/Application/Infrastructure/Tests projects for `Inventory`
4) Build and run
   - `dotnet restore && dotnet build`
   - Configure connection strings and JWT in `appsettings.Development.json`

## Structure
- `src/<SolutionName>/App.Api/` API Composition Root and shared infra
- `src/<SolutionName>/App.SharedKernel/` shared types/utilities (not domain logic)
- `src/<SolutionName>/Modules/<X>/` per-module Domain/Application/Infrastructure
- `tests/` API and module tests

## Customization guide
- Rename namespaces/solution: use `scripts/rename.sh` then inspect `.sln` and `.csproj`
- Add/remove example modules: use `scripts/new-module.sh`; delete unwanted modules and update registrations
- Swapping MongoDB: keep repository interfaces, replace Infrastructure implementations
- Eventing: keep DTO event payloads and Outbox/Inbox abstractions; wire to your transport later if needed

## Policies & Auth
Prefer policies over raw roles. Example policies: `AdminOnly`, `UserOnly`, `SelfOrAdmin`, `OwnerUserOrAdmin`.

## Notes
- This template is intentionally minimal and opinionated. It provides examples to copy/modify.
- See `doc/ARCHITECTURE.md` and `doc/TECHNICAL.md` for deeper context.

