using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Queries.GetTicketById;

internal sealed class GetTicketByIdQueryHandler(ITicketReader reader)
    : IQueryHandler<GetTicketByIdQuery, TicketResponse>
{
    public async Task<Result<TicketResponse>> Handle(GetTicketByIdQuery query, CancellationToken ct)
    {
        var ticket = await reader.GetByIdAsync(query.TicketId, ct);

        return ticket is null
            ? Result.Failure<TicketResponse>(TicketErrors.NotFound)
            : Result.Success(ticket);
    }
}
