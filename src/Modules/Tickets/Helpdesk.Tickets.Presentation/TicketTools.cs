using System.ComponentModel;
using System.Text.Json;
using Helpdesk.SharedKernel;
using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.Tickets.Application.Commands.AddChildTask;
using Helpdesk.Tickets.Application.Commands.CloseTicket;
using Helpdesk.Tickets.Application.Commands.CompleteChildTask;
using Helpdesk.Tickets.Application.Commands.CreateTicket;
using Helpdesk.Tickets.Application.Queries.GetTicketById;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Helpdesk.Tickets.Presentation;

// MCP twin of TicketsEndpoints: same handlers, results mapped to CallToolResult instead of HTTP.
[McpServerToolType]
public static class TicketTools
{
    [McpServerTool(Name = "create_ticket")]
    [Description("Opens a new helpdesk ticket with status Open. Returns only the new ticket's id as a GUID.")]
    public static async Task<CallToolResult> CreateTicket(
        [Description("Short summary of the problem, e.g. \"Printer on 3rd floor is jammed\". Must not be empty.")]
        string title,
        ICommandHandler<CreateTicketCommand, Guid> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CreateTicketCommand(title), ct);
        return result.IsSuccess ? Text(result.Value.ToString()) : Failure(result.Error);
    }

    [McpServerTool(Name = "close_ticket")]
    [Description("Closes an open ticket. Fails if the ticket does not exist, is already closed, or still has open child tasks.")]
    public static async Task<CallToolResult> CloseTicket(
        [Description("Id (GUID) of the ticket to close, as returned by create_ticket.")]
        Guid ticketId,
        ICommandHandler<CloseTicketCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CloseTicketCommand(ticketId), ct);
        return result.IsSuccess ? Text($"Ticket {ticketId} closed.") : Failure(result.Error);
    }

    [McpServerTool(Name = "add_child_task")]
    [Description("Adds a child task to an open ticket. A ticket cannot be closed until all its child tasks are completed. Returns only the new child task's id as a GUID.")]
    public static async Task<CallToolResult> AddChildTask(
        [Description("Id (GUID) of the open ticket to add the child task to.")]
        Guid ticketId,
        [Description("What needs to be done, e.g. \"Order new toner\".")]
        string description,
        ICommandHandler<AddChildTaskCommand, Guid> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new AddChildTaskCommand(ticketId, description), ct);
        return result.IsSuccess ? Text(result.Value.ToString()) : Failure(result.Error);
    }

    [McpServerTool(Name = "complete_child_task")]
    [Description("Marks a child task of an open ticket as done. Fails if the ticket does not exist, is closed, or has no child task with that id.")]
    public static async Task<CallToolResult> CompleteChildTask(
        [Description("Id (GUID) of the ticket that owns the child task.")]
        Guid ticketId,
        [Description("Id (GUID) of the child task to complete, as returned by add_child_task.")]
        Guid childTaskId,
        ICommandHandler<CompleteChildTaskCommand> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new CompleteChildTaskCommand(ticketId, childTaskId), ct);
        return result.IsSuccess ? Text($"Child task {childTaskId} completed.") : Failure(result.Error);
    }

    [McpServerTool(Name = "get_ticket", ReadOnly = true)]
    [Description("Gets one ticket by id. Returns JSON with id, title and status (Open or Closed).")]
    public static async Task<CallToolResult> GetTicket(
        [Description("Id (GUID) of the ticket to fetch, as returned by create_ticket.")]
        Guid ticketId,
        IQueryHandler<GetTicketByIdQuery, TicketResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetTicketByIdQuery(ticketId), ct);
        return result.IsSuccess ? Text(JsonSerializer.Serialize(result.Value)) : Failure(result.Error);
    }

    private static CallToolResult Text(string text) =>
        new() { Content = [new TextContentBlock { Text = text }] };

    private static CallToolResult Failure(Error error) =>
        new() { Content = [new TextContentBlock { Text = $"{error.Code}: {error.Message}" }], IsError = true };
}
