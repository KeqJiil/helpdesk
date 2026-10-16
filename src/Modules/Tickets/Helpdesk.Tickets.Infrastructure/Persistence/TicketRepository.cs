using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Tickets.Infrastructure.Persistence;

internal sealed class TicketRepository(TicketsDbContext db) : ITicketRepository
{
    // Child tasks are owned, so EF loads them with the ticket automatically.
    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct);

    public void Add(Ticket ticket) => db.Tickets.Add(ticket);
}
