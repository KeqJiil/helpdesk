using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Commands.AddChildTask;
using Helpdesk.Tickets.Application.Commands.CloseTicket;
using Helpdesk.Tickets.Application.Commands.CompleteChildTask;
using Helpdesk.Tickets.Application.Commands.CreateTicket;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Tickets.Application;

public static class TicketsApplicationExtensions
{
    /// <summary>
    /// Registers the Tickets module's command/query handlers and validators.
    /// </summary>
    public static IServiceCollection AddTicketsApplication(this IServiceCollection services)
    {
        return services
            .AddCommandHandler<CreateTicketCommand, Guid, CreateTicketCommandHandler>()
            .AddCommandHandler<CloseTicketCommand, CloseTicketCommandHandler>()
            .AddCommandHandler<AddChildTaskCommand, Guid, AddChildTaskCommandHandler>()
            .AddCommandHandler<CompleteChildTaskCommand, CompleteChildTaskCommandHandler>();
    }
}
