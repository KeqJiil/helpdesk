using Helpdesk.SharedKernel.Abstractions;

namespace Helpdesk.SharedKernel.Interceptors;

/// <summary>
/// Runs every registered <see cref="IValidator{T}"/> for the command before the handler.
/// Returns the first reported error and skips the handler when validation fails.
/// </summary>
internal sealed class ValidationInterceptor<TCommand, TResponse>(
    ICommandHandler<TCommand, TResponse> inner,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct)
    {
        foreach (var validator in validators)
        {
            var errors = validator.Validate(command);
            if (errors.Count > 0)
                return Task.FromResult(Result.Failure<TResponse>(errors[0]));
        }

        return inner.Handle(command, ct);
    }
}

/// <summary>Same as <see cref="ValidationInterceptor{TCommand, TResponse}"/>, for commands with no value.</summary>
internal sealed class ValidationInterceptor<TCommand>(
    ICommandHandler<TCommand> inner,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public Task<Result> Handle(TCommand command, CancellationToken ct)
    {
        foreach (var validator in validators)
        {
            var errors = validator.Validate(command);
            if (errors.Count > 0)
                return Task.FromResult(Result.Failure(errors[0]));
        }

        return inner.Handle(command, ct);
    }
}
