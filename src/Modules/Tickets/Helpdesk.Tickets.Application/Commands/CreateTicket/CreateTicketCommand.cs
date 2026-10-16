using Helpdesk.SharedKernel.Abstractions;

namespace Helpdesk.Tickets.Application.Commands.CreateTicket;

/// <summary>Creates a new open ticket. Returns the new ticket's id.</summary>
public sealed record CreateTicketCommand(string Title) : ICommand<Guid>;
