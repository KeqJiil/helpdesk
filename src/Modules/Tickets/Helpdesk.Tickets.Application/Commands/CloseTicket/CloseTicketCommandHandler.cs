using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Commands.CloseTicket;

internal sealed class CloseTicketCommandHandler(ITicketRepository tickets, IUnitOfWork unitOfWork)
    : ICommandHandler<CloseTicketCommand>
{
    public async Task<Result> Handle(CloseTicketCommand command, CancellationToken ct)
    {
        var ticket = await tickets.GetByIdAsync(command.TicketId, ct);
        if (ticket is null)
            return Result.Failure(TicketErrors.NotFound);

        var closed = ticket.Close();
        if (closed.IsFailure)
            return closed;

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}
