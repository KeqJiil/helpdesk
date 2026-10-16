namespace Helpdesk.SharedKernel.Abstractions;

public interface IValidator<in T>
{
    IReadOnlyList<Error> Validate(T instance);
}
