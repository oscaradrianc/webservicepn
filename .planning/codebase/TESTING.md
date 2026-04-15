# TESTING.md — Test Structure and Practices

## Current State

**No test projects exist in this solution.**

Confirmed by:
- No `*.Tests.csproj` or `*.Test.csproj` files found
- No xUnit, NUnit, or MSTest package references in any `.csproj`
- `CLAUDE.md` explicitly states: "No test projects exist in this solution"

## Coverage

- Unit test coverage: **0%**
- Integration test coverage: **0%**
- No CI test pipeline configured

## What Testing Would Look Like (if added)

### Recommended Framework

**xUnit** — industry standard for ASP.NET Core 9.0 projects.

### Suggested Structure

```
PortalNegocioWS.Tests/
  Controllers/
    SolicitudControllerTests.cs
  Business/
    SolicitudBusinessTests.cs
  Integration/
    ApiIntegrationTests.cs
```

### Mocking Approach

- Mock `IStorageService`, business interfaces via Moq or NSubstitute
- Database: use Devart LinqConnect's in-memory or test DB — avoid mocking the DataContext directly (ORM coupling risk)
- `WebApplicationFactory<Program>` for integration tests

### Key Test Targets (highest value)

1. Business layer validation logic (solicitation state transitions, period validation)
2. JWT auth / role resolution
3. Cron job execution logic in `CronJobService` subclasses
4. AutoMapper mappings in `MappingProfile.cs`

## Notes

- Business layer mixes async signatures with synchronous LINQ — tests should await normally
- Oracle connection required for integration tests (no SQLite fallback with LinqConnect)
- Background jobs (`CronJobService`) are testable if extracted to pure business methods
