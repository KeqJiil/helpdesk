using Helpdesk.SharedKernel.Abstractions;

namespace Helpdesk.Tickets.Application.Queries.GetTicketById;

/// <summary>Returns the ticket itself, without its child tasks.</summary>
public sealed record GetTicketByIdQuery(Guid TicketId) : IQuery<TicketResponse>;
