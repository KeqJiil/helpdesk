namespace Helpdesk.Tickets.Application.Queries.GetTicketChildrenById;

public sealed record ChildTaskResponse(Guid Id, string Description, bool IsDone);
