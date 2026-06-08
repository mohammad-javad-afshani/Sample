# AI Agent & Review Guidelines

This file is read automatically by **CodeRabbit** and coding agents.

**Full project context:** [docs/REVIEW_PROJECT_CONTEXT.md](docs/REVIEW_PROJECT_CONTEXT.md)

## Stack

.NET 7 · ASP.NET Core · MediatR · FluentValidation · EF Core · SQL Server

## Before approving any PR

- [ ] Input validation on all new commands (FluentValidation or domain guards)
- [ ] Auth on sensitive read/write endpoints (`ApiKeyAuth`)
- [ ] No raw SQL string interpolation
- [ ] No sensitive data in logs or public DTOs
- [ ] `SaveChangesAsync` after repository mutations
- [ ] Async I/O without `.Result` / `.GetAwaiter().GetResult()` / blocking `Task.Run`
- [ ] Pagination caps on list/export endpoints
- [ ] External calls: timeout, bounded retry, no empty catch

## Architecture

Respect layer boundaries: Domain has no EF/HTTP; Application has no `DbContext` direct use except read queries; Infrastructure implements repositories and HTTP clients.
