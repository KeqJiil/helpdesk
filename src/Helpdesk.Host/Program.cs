using Helpdesk.Tickets.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTicketsApplication();

var app = builder.Build();

app.MapGet("/health", () => "OK");

app.Run();
