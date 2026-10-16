using Helpdesk.SharedKernel.Abstractions;

namespace Helpdesk.Tickets.Application.Commands.CloseTicket;

/// <summary>Closes a ticket. Fails if any of its child tasks is still open.</summary>
public sealed record CloseTicketCommand(Guid TicketId) : ICommand;
