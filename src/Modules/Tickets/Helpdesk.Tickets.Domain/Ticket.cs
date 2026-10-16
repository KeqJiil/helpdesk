using Helpdesk.SharedKernel;
using static Helpdesk.SharedKernel.Result;

namespace Helpdesk.Tickets.Domain;

public sealed class Ticket : Entity
{
    private readonly List<ChildTask> _childTasks = new();

    private Ticket() { }   // EF Core + nobody else

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public TicketStatus Status { get; private set; }
    public IReadOnlyList<ChildTask> ChildTasks => _childTasks;

    public static Result<Ticket> Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Failure<Ticket>(TicketErrors.EmptyTitle);

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Status = TicketStatus.Open,
        };
        ticket.Raise(new TicketCreated(ticket.Id, DateTime.UtcNow));
        return Success(ticket);
    }

    public Result AddChildTask(string description)
    {
        if (Status == TicketStatus.Closed)
            return Failure(TicketErrors.AlreadyClosed);

        _childTasks.Add(new ChildTask(Guid.NewGuid(), description.Trim()));
        return Success();
    }

    public Result CompleteChildTask(Guid childTaskId)
    {
        if (Status == TicketStatus.Closed)
            return Failure(TicketErrors.AlreadyClosed);

        var childTask = _childTasks.Find(t => t.Id == childTaskId);
        if (childTask is null)
            return Failure(TicketErrors.UnknownChildTask);

        childTask.MarkDone();
        return Success();
    }

    public Result Close()
    {
        if (Status == TicketStatus.Closed)
            return Failure(TicketErrors.AlreadyClosed);

        if (_childTasks.Exists(t => !t.IsDone))
            return Failure(TicketErrors.OpenChildTasks);

        Status = TicketStatus.Closed;
        Raise(new TicketClosed(Id, DateTime.UtcNow));
        return Success();
    }
}

public enum TicketStatus { Open, Closed }
