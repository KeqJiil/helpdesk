using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Queries.GetTicketChildrenById;

internal sealed class GetTicketChildrenByIdQueryHandler(ITicketReader reader)
    : IQueryHandler<GetTicketChildrenByIdQuery, ChildTasksPage>
{
    public async Task<Result<ChildTasksPage>> Handle(GetTicketChildrenByIdQuery query, CancellationToken ct)
    {
        if (!await reader.ExistsAsync(query.TicketId, ct))
            return Result.Failure<ChildTasksPage>(TicketErrors.NotFound);

        // One extra row tells us whether another page exists without a COUNT query.
        var rows = await reader.GetChildTasksAsync(query.TicketId, query.After, query.PageSize + 1, ct);

        if (rows.Count <= query.PageSize)
            return Result.Success(new ChildTasksPage(rows, NextCursor: null));

        var items = rows.Take(query.PageSize).ToList();
        return Result.Success(new ChildTasksPage(items, NextCursor: items[^1].Id));
    }
}
