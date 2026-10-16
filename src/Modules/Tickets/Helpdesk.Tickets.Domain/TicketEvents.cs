using Helpdesk.SharedKernel;

namespace Helpdesk.Tickets.Domain;

public sealed record TicketCreated(Guid TicketId, DateTime OccurredOnUtc) : IDomainEvent;
public sealed record TicketClosed(Guid TicketId, DateTime OccurredOnUtc) : IDomainEvent;
