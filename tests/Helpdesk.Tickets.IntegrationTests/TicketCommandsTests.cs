using System.Net;
using System.Net.Http.Json;

namespace Helpdesk.Tickets.IntegrationTests;

[Collection(TicketsApiCollection.Name)]
public sealed class TicketCommandsTests(TicketsApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // CreateTicket
    [Fact]
    public async Task Create_with_title_returns_201_with_id_and_location()
    {
        var response = await _client.PostAsJsonAsync("/tickets", new { title = "Printer on fire" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotEqual(Guid.Empty, body!.Id);
        Assert.Equal($"/tickets/{body.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Create_with_blank_title_returns_400_EmptyTitle()
    {
        var response = await _client.PostAsJsonAsync("/tickets", new { title = "   " });

        await AssertProblem(response, HttpStatusCode.BadRequest, "Tickets.EmptyTitle");
    }

    // CloseTicket
    [Fact]
    public async Task Close_without_child_tasks_returns_204_then_409_when_closed_again()
    {
        var ticketId = await CreateTicket();

        var first = await _client.PostAsync($"/tickets/{ticketId}/close", null);
        var second = await _client.PostAsync($"/tickets/{ticketId}/close", null);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        await AssertProblem(second, HttpStatusCode.Conflict, "Tickets.AlreadyClosed");
    }

    [Fact]
    public async Task Close_unknown_ticket_returns_404_NotFound()
    {
        var response = await _client.PostAsync($"/tickets/{Guid.NewGuid()}/close", null);

        await AssertProblem(response, HttpStatusCode.NotFound, "Tickets.NotFound");
    }

    // AddChildTask
    [Fact]
    public async Task Add_child_task_returns_201_and_close_then_fails_with_409_OpenChildTasks()
    {
        var ticketId = await CreateTicket();

        var added = await _client.PostAsJsonAsync($"/tickets/{ticketId}/child-tasks", new { description = "Extinguish printer" });
        var close = await _client.PostAsync($"/tickets/{ticketId}/close", null);

        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        var body = await added.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotEqual(Guid.Empty, body!.Id);
        await AssertProblem(close, HttpStatusCode.Conflict, "Tickets.OpenChildTasks");
    }

    [Fact]
    public async Task Add_child_task_to_closed_ticket_returns_409_AlreadyClosed()
    {
        var ticketId = await CreateTicket();
        await _client.PostAsync($"/tickets/{ticketId}/close", null);

        var response = await _client.PostAsJsonAsync($"/tickets/{ticketId}/child-tasks", new { description = "Too late" });

        await AssertProblem(response, HttpStatusCode.Conflict, "Tickets.AlreadyClosed");
    }

    // CompleteChildTask
    [Fact]
    public async Task Complete_child_task_returns_204_and_ticket_can_then_be_closed()
    {
        var ticketId = await CreateTicket();
        var childTaskId = await AddChildTask(ticketId, "Extinguish printer");

        var completed = await _client.PostAsync($"/tickets/{ticketId}/child-tasks/{childTaskId}/complete", null);
        var close = await _client.PostAsync($"/tickets/{ticketId}/close", null);

        Assert.Equal(HttpStatusCode.NoContent, completed.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, close.StatusCode);
    }

    [Fact]
    public async Task Complete_unknown_child_task_returns_404_UnknownChildTask()
    {
        var ticketId = await CreateTicket();

        var response = await _client.PostAsync($"/tickets/{ticketId}/child-tasks/{Guid.NewGuid()}/complete", null);

        await AssertProblem(response, HttpStatusCode.NotFound, "Tickets.UnknownChildTask");
    }

    private async Task<Guid> CreateTicket()
    {
        var response = await _client.PostAsJsonAsync("/tickets", new { title = "Printer on fire" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    private async Task<Guid> AddChildTask(Guid ticketId, string description)
    {
        var response = await _client.PostAsJsonAsync($"/tickets/{ticketId}/child-tasks", new { description });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    private static async Task AssertProblem(HttpResponseMessage response, HttpStatusCode status, string errorCode)
    {
        Assert.Equal(status, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Equal(errorCode, problem!.Title);
    }

    private sealed record IdResponse(Guid Id);

    private sealed record ProblemResponse(string Title, string Detail, int Status);
}
