namespace Helpdesk.Tickets.Domain;

public sealed class ChildTask
{
    private ChildTask() { }

    internal ChildTask(Guid id, string description)
    {
        Id = id;
        Description = description;
    }

    public Guid Id { get; private set; }
    public string Description { get; private set; } = null!;
    public bool IsDone { get; private set; }

    internal void MarkDone() => IsDone = true;
}
