# .NET Modular Monolith Template

Reusable .NET 8 modular monolith template following Clean Architecture and DDD-inspired module boundaries.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-green.svg)](https://github.com/MaiQD/modular-monolith-with-ddd)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Overview

This repository provides a production-ready starting point for building modular monolith backends with:
- Clear module boundaries (Domain, Application, Infrastructure, API)
- CQRS-style handlers, FluentValidation validators, and Mapperly mappers
- A Shared Kernel with primitives (Results, Outbox/Inbox DTOs, utilities)
- MongoDB repositories with index configuration and seeders (example infra)
- Swagger/OpenAPI, health checks, and sensible defaults
- Unit/integration test scaffolding

It ships with example modules you can keep, rename, or remove. Use the scripts to rename the solution/root namespace and to scaffold new modules quickly.

## Repo structure

```
src/
   <SolutionName>/
      dotFitness.Api/                # API composition root (example name)
      dotFitness.SharedKernel/       # Cross-cutting primitives (no domain logic)
      Modules/
         Users/                       # Example module
         Exercises/                   # Example module
tests/
   api/                             # HTTP/API test examples
scripts/
   rename.sh                        # Rename solution & namespaces
   new-module.sh                    # Scaffold a new module
```

Module skeleton (per feature):
```
dotFitness.Modules.{Module}/
   dotFitness.Modules.{Module}.Domain/
   dotFitness.Modules.{Module}.Application/
   dotFitness.Modules.{Module}.Infrastructure/
   dotFitness.Modules.{Module}.Tests/
```

## Use this template

- GitHub UI: Use this template → create your repo
- CLI:
   - gh repo create <owner>/<repo> --template MaiQD/dotnet-modular-monolith-template --public

Then optionally run the rename script to set your own root namespace and solution name.

```
scripts/rename.sh MyCompany.MyApp MyApp.Monolith
```

## Quick start (local)

Prerequisites:
- .NET 8 SDK
- Docker (for local MongoDB) or a MongoDB instance

Steps:
1) Start MongoDB (optional if you already have one):
    - From solution folder with docker-compose.yml:
       - src/dotFitness.WorkoutTracker/docker-compose.yml
2) Restore, build, run:
    - dotnet restore
    - dotnet build
    - dotnet run --project src/dotFitness.WorkoutTracker/dotFitness.Api

When the API starts, check the console for the URLs (Swagger UI is served at the root by default in this template). Health endpoint is typically /health.

Configuration (development) lives in `src/dotFitness.WorkoutTracker/dotFitness.Api/appsettings.Development.json`. Common keys:
- ConnectionStrings:MongoDB
- JwtSettings:SecretKey, Issuer, Audience, ExpirationHours

Environment variables can override config values, e.g. `ConnectionStrings__MongoDB`.

## Docs

- Architecture: doc/ARCHITECTURE.md
- Technical Notes: doc/TECHNICAL.md
- Add a Module: doc/ADD_NEW_MODULE.md
- Step-by-step Dev Guide: doc/STEP_BY_STEP.md

Example product docs (UI/URS/feature lists) have been removed to keep this template generic.

## Customization

- Rename solution/namespaces: scripts/rename.sh
- Scaffold a module: scripts/new-module.sh <ModuleName> <RootNamespace>
- Swap MongoDB: keep repository interfaces, replace Infrastructure implementations
- Eventing: Outbox/Inbox DTOs included; wire to your transport if needed later

## Testing

- Run all tests: dotnet test
- Add per-module test projects under dotFitness.Modules.{Module}.Tests

## License

MIT — see LICENSE.

## Acknowledgments

- Modular Monolith with DDD (inspiration)
- Clean Architecture principles
