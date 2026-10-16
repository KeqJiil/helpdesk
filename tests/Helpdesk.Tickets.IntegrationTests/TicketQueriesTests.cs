using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Helpdesk.Tickets.IntegrationTests;

[Collection(TicketsApiCollection.Name)]
public sealed class TicketQueriesTests(TicketsApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // GetTicketById
    [Fact]
    public async Task Get_ticket_returns_200_with_ticket_fields_only()
    {
        var ticketId = await CreateTicket("Printer on fire");
        await AddChildTask(ticketId, "Extinguish printer");

        var response = await _client.GetAsync($"/tickets/{ticketId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var properties = json.RootElement.EnumerateObject().Select(p => p.Name).Order().ToArray();
        Assert.Equal(["id", "status", "title"], properties);
        Assert.Equal(ticketId, json.RootElement.GetProperty("id").GetGuid());
        Assert.Equal("Printer on fire", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Open", json.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Get_unknown_ticket_returns_404_NotFound()
    {
        var response = await _client.GetAsync($"/tickets/{Guid.NewGuid()}");

        await AssertProblem(response, HttpStatusCode.NotFound, "Tickets.NotFound");
    }

    // GetTicketChildrenById
    [Fact]
    public async Task Get_children_pages_through_all_child_tasks_without_gaps_or_duplicates()
    {
        var ticketId = await CreateTicket("Printer on fire");
        var added = new List<Guid>();
        for (var i = 1; i <= 12; i++)
            added.Add(await AddChildTask(ticketId, $"Task {i}"));

        var pageSizes = new List<int>();
        var seen = new List<Guid>();
        Guid? after = null;
        do
        {
            var url = $"/tickets/{ticketId}/child-tasks?pageSize=5" + (after is null ? "" : $"&after={after}");
            var page = await _client.GetFromJsonAsync<PageResponse>(url);
            pageSizes.Add(page!.Items.Count);
            seen.AddRange(page.Items.Select(c => c.Id));
            after = page.NextCursor;
        } while (after is not null);

        Assert.Equal([5, 5, 2], pageSizes);
        Assert.Equal(added.Order(), seen.Order());
        Assert.Equal(seen.Count, seen.Distinct().Count());
    }

    [Fact]
    public async Task Get_children_without_page_size_uses_default()
    {
        var ticketId = await CreateTicket("Printer on fire");
        await AddChildTask(ticketId, "Extinguish printer");

        var page = await _client.GetFromJsonAsync<PageResponse>($"/tickets/{ticketId}/child-tasks");

        var item = Assert.Single(page!.Items);
        Assert.Equal("Extinguish printer", item.Description);
        Assert.False(item.IsDone);
        Assert.Null(page.NextCursor);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(101)]
    [InlineData(0)]
    public async Task Get_children_with_page_size_outside_5_to_100_returns_400_InvalidPageSize(int pageSize)
    {
        var ticketId = await CreateTicket("Printer on fire");

        var response = await _client.GetAsync($"/tickets/{ticketId}/child-tasks?pageSize={pageSize}");

        await AssertProblem(response, HttpStatusCode.BadRequest, "Tickets.InvalidPageSize");
    }

    [Theory]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Get_children_with_page_size_on_the_bounds_returns_200(int pageSize)
    {
        var ticketId = await CreateTicket("Printer on fire");

        var response = await _client.GetAsync($"/tickets/{ticketId}/child-tasks?pageSize={pageSize}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_children_of_unknown_ticket_returns_404_NotFound()
    {
        var response = await _client.GetAsync($"/tickets/{Guid.NewGuid()}/child-tasks");

        await AssertProblem(response, HttpStatusCode.NotFound, "Tickets.NotFound");
    }

    private async Task<Guid> CreateTicket(string title)
    {
        var response = await _client.PostAsJsonAsync("/tickets", new { title });
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

    private sealed record ChildTaskItem(Guid Id, string Description, bool IsDone);

    private sealed record PageResponse(List<ChildTaskItem> Items, Guid? NextCursor);

    private sealed record ProblemResponse(string Title, string Detail, int Status);
}
