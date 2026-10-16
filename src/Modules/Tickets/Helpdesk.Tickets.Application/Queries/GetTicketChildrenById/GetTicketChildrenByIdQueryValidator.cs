using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;

namespace Helpdesk.Tickets.Application.Queries.GetTicketChildrenById;

internal sealed class GetTicketChildrenByIdQueryValidator : IValidator<GetTicketChildrenByIdQuery>
{
    public const int MinPageSize = 5;
    public const int MaxPageSize = 100;

    public static readonly Error InvalidPageSize =
        new("Tickets.InvalidPageSize", $"Page size must be between {MinPageSize} and {MaxPageSize}.");

    public IReadOnlyList<Error> Validate(GetTicketChildrenByIdQuery query) =>
        query.PageSize is < MinPageSize or > MaxPageSize ? [InvalidPageSize] : [];
}
