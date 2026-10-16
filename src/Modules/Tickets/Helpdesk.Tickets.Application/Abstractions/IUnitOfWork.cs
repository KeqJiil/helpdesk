namespace Helpdesk.Tickets.Application.Abstractions;

/// <summary>
/// Commits all changes tracked by the Tickets module's repositories in one transaction.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
