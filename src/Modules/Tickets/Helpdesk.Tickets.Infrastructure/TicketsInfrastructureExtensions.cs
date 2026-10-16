using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Tickets.Infrastructure;

public static class TicketsInfrastructureExtensions
{
    /// <summary>
    /// Registers the Tickets module's DbContext, unit of work and repositories.
    /// </summary>
    public static IServiceCollection AddTicketsInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<TicketsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, TicketsDbContext.Schema)));

        // Same scoped DbContext instance the repositories use, so SaveChanges commits their changes.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TicketsDbContext>());
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketReader, TicketReader>();

        return services;
    }
}
