using Helpdesk.Tickets.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Helpdesk.Tickets.IntegrationTests;

/// <summary>
/// Hosts the real app in memory against a throwaway SQL Server container with migrations applied.
/// </summary>
public sealed class TicketsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sql =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _sql.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketsDbContext>();
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" keeps appsettings.Development.json (and its connection string) out of the picture.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Tickets", _sql.GetConnectionString());
    }

    // WebApplicationFactory.DisposeAsync returns ValueTask; xunit's IAsyncLifetime wants Task.
    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _sql.DisposeAsync();
    }
}
