namespace Helpdesk.Tickets.Application.Queries.GetTicketChildrenById;

/// <param name="Items">The child tasks on this page.</param>
/// <param name="NextCursor">Id to pass as <c>after</c> for the next page; <c>null</c> on the last page.</param>
public sealed record ChildTasksPage(IReadOnlyList<ChildTaskResponse> Items, Guid? NextCursor);
