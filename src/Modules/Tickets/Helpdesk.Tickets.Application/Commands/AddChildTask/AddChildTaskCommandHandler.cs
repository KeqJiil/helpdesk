using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Commands.AddChildTask;

internal sealed class AddChildTaskCommandHandler(ITicketRepository tickets, IUnitOfWork unitOfWork)
    : ICommandHandler<AddChildTaskCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddChildTaskCommand command, CancellationToken ct)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, ct);
        if (ticket is null)
            return Result.Failure<Guid>(TicketErrors.NotFound);

        var added = ticket.AddChildTask(command.Description);
        if (added.IsFailure)
            return added;

        await unitOfWork.SaveChangesAsync(ct);

        return added;
    }
}
