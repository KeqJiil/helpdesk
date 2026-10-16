using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Commands.CreateTicket;

internal sealed class CreateTicketCommandHandler(ITicketRepository tickets, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTicketCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicketCommand command, CancellationToken ct)
    {
        var created = Ticket.Create(command.Title);
        if (created.IsFailure)
            return Result.Failure<Guid>(created.Error);

        tickets.Add(created.Value);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(created.Value.Id);
    }
}
