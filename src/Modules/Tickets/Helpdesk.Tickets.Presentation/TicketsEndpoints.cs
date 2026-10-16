using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.Tickets.Application.Commands.AddChildTask;
using Helpdesk.Tickets.Application.Commands.CloseTicket;
using Helpdesk.Tickets.Application.Commands.CompleteChildTask;
using Helpdesk.Tickets.Application.Commands.CreateTicket;
using Helpdesk.Tickets.Application.Queries.GetTicketById;
using Helpdesk.Tickets.Application.Queries.GetTicketChildrenById;
using Helpdesk.Tickets.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Helpdesk.Tickets.Presentation;

public sealed record CreateTicketRequest(string Title);
public sealed record AddChildTaskRequest(string Description);

public static class TicketsEndpoints
{
    public static IEndpointRouteBuilder MapTicketsEndpoints(this IEndpointRouteBuilder app)
    {
        var tickets = app.MapGroup("/tickets").WithTags("Tickets");

        tickets.MapPost("/", async (CreateTicketRequest request,
            CreateTicketCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateTicketCommand(request.Title), ct);
            return result.IsSuccess
                ? Results.Created($"/tickets/{result.Value}", new { id = result.Value })
                : ToProblem(result.Error);
        });

        tickets.MapPost("/{ticketId:guid}/close", async (Guid ticketId,
            [FromServices] ICommandHandler<CloseTicketCommand> handler, CancellationToken ct) =>
            ToResponse(await handler.Handle(new CloseTicketCommand(ticketId), ct)));

        tickets.MapPost("/{ticketId:guid}/child-tasks", async (Guid ticketId, AddChildTaskRequest request,
            [FromServices] ICommandHandler<AddChildTaskCommand, Guid> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new AddChildTaskCommand(ticketId, request.Description), ct);
            return result.IsSuccess
                ? Results.Created($"/tickets/{ticketId}/child-tasks/{result.Value}", new { id = result.Value })
                : ToProblem(result.Error);
        });

        tickets.MapPost("/{ticketId:guid}/child-tasks/{childTaskId:guid}/complete", async (Guid ticketId, Guid childTaskId,
            [FromServices] ICommandHandler<CompleteChildTaskCommand> handler, CancellationToken ct) =>
            ToResponse(await handler.Handle(new CompleteChildTaskCommand(ticketId, childTaskId), ct)));

        tickets.MapGet("/{ticketId:guid}", async (Guid ticketId,
            [FromServices] IQueryHandler<GetTicketByIdQuery, TicketResponse> handler, CancellationToken ct) =>
            ToOk(await handler.Handle(new GetTicketByIdQuery(ticketId), ct)));

        // Keyset pagination: pass the previous page's nextCursor as ?after= to get the next page.
        tickets.MapGet("/{ticketId:guid}/child-tasks", async (Guid ticketId, Guid? after, int? pageSize,
            [FromServices] IQueryHandler<GetTicketChildrenByIdQuery, ChildTasksPage> handler, CancellationToken ct) =>
            ToOk(await handler.Handle(new GetTicketChildrenByIdQuery(
                ticketId, after, pageSize ?? GetTicketChildrenByIdQuery.DefaultPageSize), ct)));

        return app;
    }

    // Queries: 200 with the value on success.
    private static IResult ToOk<T>(Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);

    // No-value commands: 204 on success.
    private static IResult ToResponse(Result result) =>
        result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);

    private static IResult ToProblem(Error error)
    {
        var status = error switch
        {
            _ when error == TicketErrors.NotFound || error == TicketErrors.UnknownChildTask
                => StatusCodes.Status404NotFound,
            _ when error == TicketErrors.AlreadyClosed || error == TicketErrors.OpenChildTasks
                => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        return Results.Problem(title: error.Code, detail: error.Message, statusCode: status);
    }
}
