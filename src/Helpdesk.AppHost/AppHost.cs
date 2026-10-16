var builder = DistributedApplication.CreateBuilder(args);

// Persistent container with a data volume, so data survives AppHost restarts.
// No fixed host port: Aspire picks a free one. Host port 14330 belongs to the
// docker compose SQL Server from the earlier labs, which must keep running.
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .WithContainerName("helpdesk-aspire-sql");

// Resource "Tickets" -> ConnectionStrings__Tickets in the api; physical database HelpdeskTickets.
var tickets = sql.AddDatabase("Tickets", databaseName: "HelpdeskTickets");

builder.AddProject<Projects.Helpdesk_Host>("api")
    .WithReference(tickets)
    .WaitFor(tickets)
    .WithEnvironment("Migrations__ApplyOnStartup", "true");

builder.Build().Run();
