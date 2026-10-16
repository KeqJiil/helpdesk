using Helpdesk.Tickets.Application;
using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;

var builder = WebApplication.CreateBuilder(args);

var ticketsConnectionString = builder.Configuration.GetConnectionString("Tickets")
    ?? throw new InvalidOperationException("Connection string 'Tickets' is not configured.");

builder.Services
    .AddTicketsApplication()
    .AddTicketsInfrastructure(ticketsConnectionString);

var app = builder.Build();

app.MapGet("/health", () => "OK");
app.MapTicketsEndpoints();

app.Run();

// Exposes the entry point to WebApplicationFactory<Program> in integration tests.
public partial class Program;
