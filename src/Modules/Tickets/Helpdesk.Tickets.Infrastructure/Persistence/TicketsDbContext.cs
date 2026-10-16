using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Tickets.Infrastructure.Persistence;

internal sealed class TicketsDbContext(DbContextOptions<TicketsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal const string Schema = "tickets";

    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketsDbContext).Assembly);
    }

    // DbContext.SaveChangesAsync returns Task<int>; IUnitOfWork exposes plain Task.
    Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);
}
