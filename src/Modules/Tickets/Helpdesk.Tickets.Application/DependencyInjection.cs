using Helpdesk.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Tickets.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Tickets module's command/query handlers and validators.
    /// Add each new handler here, e.g.
    /// <c>services.AddCommandHandler&lt;CloseTicketCommand, Guid, CloseTicketCommandHandler&gt;();</c>
    /// </summary>
    public static IServiceCollection AddTicketsApplication(this IServiceCollection services)
    {
        return services;
    }
}
