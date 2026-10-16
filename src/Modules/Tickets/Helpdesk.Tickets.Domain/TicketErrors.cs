using Helpdesk.SharedKernel;

namespace Helpdesk.Tickets.Domain;

public static class TicketErrors
{
    public static readonly Error EmptyTitle =
        new("Tickets.EmptyTitle", "A ticket needs a non-empty title.");
    public static readonly Error AlreadyClosed =
        new("Tickets.AlreadyClosed", "The ticket is already closed.");
    public static readonly Error OpenChildTasks =
        new("Tickets.OpenChildTasks", "Close all child tasks before closing the ticket.");
    public static readonly Error UnknownChildTask =
        new("Tickets.UnknownChildTask", "No such child task on this ticket.");
    public static readonly Error NotFound =
        new("Tickets.NotFound", "Ticket not found.");
}
