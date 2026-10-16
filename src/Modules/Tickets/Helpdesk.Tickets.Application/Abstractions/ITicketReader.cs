using Helpdesk.Tickets.Application.Queries.GetTicketById;
using Helpdesk.Tickets.Application.Queries.GetTicketChildrenById;

namespace Helpdesk.Tickets.Application.Abstractions;

/// <summary>
/// Read-only access for queries. Returns response models, never the aggregate, and tracks nothing.
/// Implemented in Helpdesk.Tickets.Infrastructure.
/// </summary>
public interface ITicketReader
{
    /// <summary>The ticket without its child tasks, or <c>null</c> if it does not exist.</summary>
    Task<TicketResponse?> GetByIdAsync(Guid ticketId, CancellationToken ct);

    Task<bool> ExistsAsync(Guid ticketId, CancellationToken ct);

    /// <summary>
    /// Up to <paramref name="take"/> child tasks of the ticket ordered by id, starting after <paramref name="after"/>
    /// (or from the first one when <c>null</c>).
    /// </summary>
    Task<IReadOnlyList<ChildTaskResponse>> GetChildTasksAsync(Guid ticketId, Guid? after, int take, CancellationToken ct);
}
