---
name: slice
description: Use when asked to add a new command or query slice to a module, wire up a new endpoint, or implement a new use case end to end through Application, Presentation and registration.
---

# Add a vertical slice

Add one command or query, its handler, its validator, its endpoint, its registration and its tests.
One use case per slice. Follow CLAUDE.md and AGENTS.md exactly; they are the contract.
Work in small steps and pause between them, as CLAUDE.md requires.

## Steps

1. Read CLAUDE.md, then read the nearest existing slice and copy its shape:
   commands in `src/Modules/<Module>/Helpdesk.<Module>.Application/Commands/<UseCase>/`,
   queries in `.../Application/Queries/<UseCase>/`.
   The existing code is the specification; do not invent a second way of doing this.
2. Create the Application files in that folder, one type per file:
   - `<UseCase>Command.cs` / `<UseCase>Query.cs`: a public sealed record.
     A command that returns a value implements `ICommand<TResponse>`; a command with no value
     implements `ICommand` (its handler returns plain `Result`). A query implements `IQuery<TResponse>`.
   - `<UseCase>CommandHandler.cs` / `<UseCase>QueryHandler.cs`: internal, sealed, thin.
     - Commands load or create through `I<Aggregate>Repository`, call the domain behavior,
       return its failure unchanged, and save once through `IUnitOfWork` on success only.
     - Queries read through `I<Aggregate>Reader` (Application/Abstractions, implemented in
       Infrastructure with `AsNoTracking` projections). Never load the aggregate for a query.
   - Response records the query returns (e.g. `TicketResponse.cs`), one per file.
3. When the input has a shape to check, add `<UseCase><Command|Query>Validator.cs` implementing
   `IValidator<T>` (internal, sealed). Validators check input shape only; define their `Error`
   as a static field on the validator (code `<Module>.<Name>`, e.g. `Tickets.InvalidPageSize`).
   Business rules belong in the domain entity and come back as a `Result` failure, never thrown.
   The validation interceptor runs validators for both commands and queries.
4. If a query needs data the reader does not expose yet, add the method to `I<Aggregate>Reader`
   and implement it in `<Aggregate>Reader` in Infrastructure/Persistence. Project only the
   columns you return. For lists, use keyset pagination (`after` cursor + `take`), not OFFSET.
5. Add the minimal-API endpoint in
   `src/Modules/<Module>/Helpdesk.<Module>.Presentation/<Aggregate>Endpoints.cs`, resolving
   `ICommandHandler<...>` / `IQueryHandler<...>` with `[FromServices]`. Map the `Result` with the
   existing helpers:
   - success: 201 Created with `{ id }` and a Location header for a create, 204 No Content for a
     no-value command (`ToResponse`), 200 OK with the value for a query (`ToOk`);
   - failure (`ToProblem`): 404 for not-found errors, 409 for state conflicts, 400 otherwise,
     as ProblemDetails with `title` = error code and `detail` = message. Add any new domain
     error to the 404/409 cases in `ToProblem` when it belongs there.
6. Register in the module's existing extension
   (`TicketsApplicationExtensions.AddTicketsApplication()` for Tickets):
   `AddCommandHandler<TCommand, TResponse, THandler>()`, `AddCommandHandler<TCommand, THandler>()`
   for no-value commands, `AddQueryHandler<TQuery, TResponse, THandler>()`, and
   `AddValidator<T, TValidator>()`. New reader/repository implementations are registered in
   `TicketsInfrastructureExtensions.AddTicketsInfrastructure()`. Do not add a new registration file.
7. Add tests:
   - domain rules in `tests/Helpdesk.<Module>.Domain.Tests`;
   - status codes and behavior through HTTP in `tests/Helpdesk.<Module>.IntegrationTests`
     (`[Collection(TicketsApiCollection.Name)]`, `TicketsApiFactory` runs the real host against a
     Testcontainers SQL Server with migrations applied). Each test creates its own data.
     Cover success, each failure code, and validator bounds.
8. Verify: make sure Docker is running, then `dotnet build Helpdesk.slnx` and
   `dotnet test Helpdesk.slnx`. Show the output. Warnings are errors, so a build that reports any
   warning is a failure.

## Constraints

- Do not modify SharedKernel, the Domain layer, or an existing test to make a slice pass without
  the developer's explicit OK; ask first and say why.
- Never add a NuGet package without asking. Never change the database schema without a migration
  (`dotnet ef migrations add <Name> --project src/Modules/<Module>/Helpdesk.<Module>.Infrastructure
  --startup-project src/Helpdesk.Host --output-dir Persistence/Migrations`), and read the
  generated migration before reporting it.
- Do not commit, push, or touch branches.
- If a step conflicts with an existing convention, stop and ask rather than improvising.
