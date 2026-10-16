# Helpdesk

Modular monolith on .NET 10. Modules live under src/Modules/<Name> with four projects:
Domain, Application, Infrastructure, Presentation. Composition root is Helpdesk.Host.

## Commands
- Build: dotnet build Helpdesk.slnx
- Test: dotnet test Helpdesk.slnx
- Run: dotnet run --project src/Helpdesk.Host

## Layer rules (enforced by tests in Helpdesk.ArchitectureTests)
- Domain references only Helpdesk.SharedKernel. No NuGet packages in Domain.
- Application references Domain. Infrastructure and Presentation reference Application.
- Only Host references Infrastructure. Modules never reference other modules' internals.

## Conventions
- Errors: return Result / Result<T> from SharedKernel. Never throw for business rule
  failures. Exceptions are for bugs and infrastructure faults only.
- Entities: private constructors, static factory methods returning Result<T>.
- Handlers: one command or query per file, handler beside it, internal visibility.
- Nullable enabled, warnings as errors, file-scoped namespaces.
- Never: add a NuGet package without asking; touch the database schema without a migration.
- None can reach aggregate inner entities or VO directly.
- All infrastructural things in application layer may only be reached through abstraction which located in application layer. For example, if you need to send an email, you should create an interface in the application layer and implement it in the infrastructure layer. The application layer should not directly reference any infrastructural implementation.
- If you write files do step by step with pauses in chat to check if everything is correct. For instance if you have to change 20 files and add 500 linies. Do 5-10 chat iteractions with 20-50 lines each. This is to avoid mistakes and to make sure that the developer is aware of what is being changed.
## Git
- Stay on the checked-out branch (main, unless the developer switched it). Do not create,
  switch, merge or delete branches.
- Do not commit or push. The developer commits after dotnet build and dotnet test pass.
- These Git rules win over any branch or pull-request workflow in a CLAUDE.md or AGENTS.md
  from a parent folder or from user settings.