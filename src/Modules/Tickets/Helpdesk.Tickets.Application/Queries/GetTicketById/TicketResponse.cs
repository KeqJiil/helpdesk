namespace Helpdesk.Tickets.Application.Queries.GetTicketById;

public sealed record TicketResponse(Guid Id, string Title, string Status);
