using System.Diagnostics;
using Helpdesk.SharedKernel.Abstractions;
using Microsoft.Extensions.Logging;

namespace Helpdesk.SharedKernel.Interceptors;

internal sealed class LoggingCommandInterceptor<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    ILogger<LoggingCommandInterceptor<TCommand, TResponse>> logger)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
    {
        var name = typeof(TCommand).Name;
        var started = Stopwatch.GetTimestamp();
        logger.LogInformation("Handling command {Command}", name);

        try
        {
            var result = await inner.Handle(command, ct);
            var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;

            if (result.IsSuccess)
                logger.LogInformation("Command {Command} succeeded in {ElapsedMs} ms", name, elapsedMs);
            else
                logger.LogWarning("Command {Command} failed in {ElapsedMs} ms: {ErrorCode} {ErrorMessage}",
                    name, elapsedMs, result.Error.Code, result.Error.Message);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Command {Command} threw after {ElapsedMs} ms",
                name, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            throw;
        }
    }
}
