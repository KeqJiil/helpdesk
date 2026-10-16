using Helpdesk.SharedKernel.Abstractions;

namespace Helpdesk.Tickets.Application.Commands.AddChildTask;

/// <summary>Adds a child task to an open ticket. Returns the new child task's id.</summary>
public sealed record AddChildTaskCommand(Guid TicketId, string Description) : ICommand<Guid>;
