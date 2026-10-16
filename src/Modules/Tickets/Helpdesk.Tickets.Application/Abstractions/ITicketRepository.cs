using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Abstractions;

/// <summary>
/// Persistence for the <see cref="Ticket"/> aggregate. Loads and stores the whole aggregate,
/// including its child tasks. Changes are committed through <see cref="IUnitOfWork"/>.
/// </summary>
public interface ITicketRepository
{
    /// <summary>Returns the ticket, or <c>null</c> if it does not exist.</summary>
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct);

    void Add(Ticket ticket);
}
