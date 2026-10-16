using Helpdesk.Tickets.Application;
using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Infrastructure.Persistence;
using Helpdesk.Tickets.Presentation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var ticketsConnectionString = builder.Configuration.GetConnectionString("Tickets")
    ?? throw new InvalidOperationException("Connection string 'Tickets' is not configured.");

builder.Services
    .AddTicketsApplication()
    .AddTicketsInfrastructure(ticketsConnectionString);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TicketsDbContext>("tickets-db");

builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithToolsFromAssembly(typeof(TicketTools).Assembly);

var app = builder.Build();

// Off by default; the Aspire AppHost turns it on with Migrations__ApplyOnStartup=true.
if (app.Configuration.GetValue<bool>("Migrations:ApplyOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<TicketsDbContext>().Database.MigrateAsync();
}

app.MapDefaultEndpoints();
app.MapTicketsEndpoints();
app.MapMcp("/mcp");

app.Run();

// Exposes the entry point to WebApplicationFactory<Program> in integration tests.
public partial class Program;
