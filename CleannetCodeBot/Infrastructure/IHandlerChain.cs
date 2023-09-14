using CSharpFunctionalExtensions;

namespace CleannetCodeBot.Infrastructure;

public interface IHandlerChain
{
    int OrderInChain { get; }

    Task<Result> HandleAsync(TelegramRequest request, CancellationToken cancellationToken = default);
}