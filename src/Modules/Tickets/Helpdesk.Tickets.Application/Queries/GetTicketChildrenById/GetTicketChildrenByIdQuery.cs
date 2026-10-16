using Helpdesk.SharedKernel.Abstractions;

namespace Helpdesk.Tickets.Application.Queries.GetTicketChildrenById;

/// <summary>
/// One page of a ticket's child tasks, keyset-paginated by child task id.
/// Pass the previous page's <see cref="ChildTasksPage.NextCursor"/> as <paramref name="After"/> to get the next one.
/// </summary>
public sealed record GetTicketChildrenByIdQuery(Guid TicketId, Guid? After, int PageSize) : IQuery<ChildTasksPage>
{
    public const int DefaultPageSize = 20;
}
