namespace Helpdesk.SharedKernel.Abstractions;

/// <summary>A command that returns no value; its handler returns a plain <see cref="Result"/>.</summary>
public interface ICommand { }

public interface ICommand<TResponse> { }
