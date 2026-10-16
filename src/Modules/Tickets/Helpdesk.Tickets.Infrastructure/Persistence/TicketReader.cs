using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Application.Queries.GetTicketById;
using Helpdesk.Tickets.Application.Queries.GetTicketChildrenById;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Tickets.Infrastructure.Persistence;

internal sealed class TicketReader(TicketsDbContext db) : ITicketReader
{
    public async Task<TicketResponse?> GetByIdAsync(Guid ticketId, CancellationToken ct)
    {
        // Projecting scalar columns only keeps the owned ChildTasks out of the SQL.
        var row = await db.Tickets.AsNoTracking()
            .Where(t => t.Id == ticketId)
            .Select(t => new { t.Id, t.Title, t.Status })
            .FirstOrDefaultAsync(ct);

        return row is null ? null : new TicketResponse(row.Id, row.Title, row.Status.ToString());
    }

    public Task<bool> ExistsAsync(Guid ticketId, CancellationToken ct) =>
        db.Tickets.AnyAsync(t => t.Id == ticketId, ct);

    public async Task<IReadOnlyList<ChildTaskResponse>> GetChildTasksAsync(
        Guid ticketId, Guid? after, int take, CancellationToken ct)
    {
        // Owned types cannot be queried on their own, so go through the owning ticket.
        var childTasks = db.Tickets.AsNoTracking()
            .Where(t => t.Id == ticketId)
            .SelectMany(t => t.ChildTasks);

        // Keyset: seek past the cursor instead of OFFSET, so deep pages cost the same as the first.
        if (after is { } cursor)
            childTasks = childTasks.Where(c => c.Id.CompareTo(cursor) > 0);

        return await childTasks
            .OrderBy(c => c.Id)
            .Take(take)
            .Select(c => new ChildTaskResponse(c.Id, c.Description, c.IsDone))
            .ToListAsync(ct);
    }
}
