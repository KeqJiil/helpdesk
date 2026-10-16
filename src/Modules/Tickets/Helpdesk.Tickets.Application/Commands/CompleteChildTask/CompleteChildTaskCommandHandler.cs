using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Commands.CompleteChildTask;

internal sealed class CompleteChildTaskCommandHandler(ITicketRepository tickets, IUnitOfWork unitOfWork)
    : ICommandHandler<CompleteChildTaskCommand>
{
    public async Task<Result> Handle(CompleteChildTaskCommand command, CancellationToken ct)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, ct);
        if (ticket is null)
            return Result.Failure(TicketErrors.NotFound);

        var completed = ticket.CompleteChildTask(command.ChildTaskId);
        if (completed.IsFailure)
            return completed;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
