using Helpdesk.SharedKernel.Abstractions;

namespace Helpdesk.Tickets.Application.Commands.CompleteChildTask;

/// <summary>Marks a ticket's child task as done.</summary>
public sealed record CompleteChildTaskCommand(Guid TicketId, Guid ChildTaskId) : ICommand;
