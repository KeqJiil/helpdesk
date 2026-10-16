using Helpdesk.SharedKernel.Abstractions;
using Helpdesk.SharedKernel.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Helpdesk.SharedKernel;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a command handler wrapped as: logging → validation → handler.
    /// </summary>
    public static IServiceCollection AddCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(sp =>
        {
            ICommandHandler<TCommand, TResponse> handler = sp.GetRequiredService<THandler>();
            handler = new ValidationInterceptor<TCommand, TResponse>(
                handler, sp.GetServices<IValidator<TCommand>>());
            return new LoggingCommandInterceptor<TCommand, TResponse>(
                handler, sp.GetRequiredService<ILogger<LoggingCommandInterceptor<TCommand, TResponse>>>());
        });
        return services;
    }

    /// <summary>
    /// Registers a no-value command handler wrapped as: logging → validation → handler.
    /// </summary>
    public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(sp =>
        {
            ICommandHandler<TCommand> handler = sp.GetRequiredService<THandler>();
            handler = new ValidationInterceptor<TCommand>(
                handler, sp.GetServices<IValidator<TCommand>>());
            return new LoggingCommandInterceptor<TCommand>(
                handler, sp.GetRequiredService<ILogger<LoggingCommandInterceptor<TCommand>>>());
        });
        return services;
    }

    /// <summary>
    /// Registers a query handler wrapped as: logging → validation → handler.
    /// </summary>
    public static IServiceCollection AddQueryHandler<TQuery, TResponse, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResponse>>(sp =>
        {
            IQueryHandler<TQuery, TResponse> handler = sp.GetRequiredService<THandler>();
            handler = new ValidationQueryInterceptor<TQuery, TResponse>(
                handler, sp.GetServices<IValidator<TQuery>>());
            return new LoggingQueryInterceptor<TQuery, TResponse>(
                handler, sp.GetRequiredService<ILogger<LoggingQueryInterceptor<TQuery, TResponse>>>());
        });
        return services;
    }

    public static IServiceCollection AddValidator<T, TValidator>(this IServiceCollection services)
        where TValidator : class, IValidator<T>
    {
        services.AddScoped<IValidator<T>, TValidator>();
        return services;
    }
}
