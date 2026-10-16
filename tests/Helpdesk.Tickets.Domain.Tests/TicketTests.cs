namespace Helpdesk.Tickets.Domain.Tests;

public class TicketTests
{
    [Fact]
    public void CreateTicket_WithValidTitle_ShouldSucceed()
    {
        // Arrange
        var title = "Sample Ticket";

        // Act
        var result = Ticket.Create(title);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(title, result.Value.Title);
        Assert.Equal(TicketStatus.Open, result.Value.Status);
    }

    [Fact]
    public void CreateTicket_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var title = "";

        // Act
        var result = Ticket.Create(title);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TicketErrors.EmptyTitle, result.Error);
    }
}