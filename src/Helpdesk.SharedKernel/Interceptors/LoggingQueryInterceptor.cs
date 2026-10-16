using System.Diagnostics;
using Helpdesk.SharedKernel.Abstractions;
using Microsoft.Extensions.Logging;

namespace Helpdesk.SharedKernel.Interceptors;

internal sealed class LoggingQueryInterceptor<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> inner,
    ILogger<LoggingQueryInterceptor<TQuery, TResponse>> logger)
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct)
    {
        var name = typeof(TQuery).Name;
        var started = Stopwatch.GetTimestamp();
        logger.LogInformation("Handling query {Query}", name);

        try
        {
            var result = await inner.Handle(query, ct);
            var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;

            if (result.IsSuccess)
                logger.LogInformation("Query {Query} succeeded in {ElapsedMs} ms", name, elapsedMs);
            else
                logger.LogWarning("Query {Query} failed in {ElapsedMs} ms: {ErrorCode} {ErrorMessage}",
                    name, elapsedMs, result.Error.Code, result.Error.Message);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Query {Query} threw after {ElapsedMs} ms",
                name, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            throw;
        }
    }
}
